using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchedulingService.BLL.Repositories;
using SchedulingService.Data;
using SchedulingService.DTOs.ScheduledInterviews;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.BLL.Services;

public sealed class ScheduleInterviewsService
{
    private readonly InterviewSlotsRepository _interviewSlotsRepository;
    private readonly ScheduledInterviewsRepository _scheduledInterviewsRepository;
    private readonly StudentAvailabilityRepository _studentAvailabilityRepository;
    private readonly SchedulingDbContext _db;
    private readonly IMapper _mapper;

    public ScheduleInterviewsService(
        InterviewSlotsRepository interviewSlotsRepository,
        ScheduledInterviewsRepository scheduledInterviewsRepository,
        StudentAvailabilityRepository studentAvailabilityRepository,
        SchedulingDbContext db,
        IMapper mapper)
    {
        _interviewSlotsRepository = interviewSlotsRepository;
        _scheduledInterviewsRepository = scheduledInterviewsRepository;
        _studentAvailabilityRepository = studentAvailabilityRepository;
        _db = db;
        _mapper = mapper;
    }

    /// <summary>
    /// Schedules an interview for a student into a specific interview slot, enforcing slot availability and student availability rules.
    /// Creates a <see cref="ScheduledInterview"/> and marks the slot as assigned atomically.
    /// </summary>
    public async Task<ScheduledInterviewDto> ScheduleInterviewAsync(ScheduleInterviewDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (dto.StudentId <= 0) throw new ArgumentOutOfRangeException(nameof(dto.StudentId));
        if (dto.SlotId <= 0) throw new ArgumentOutOfRangeException(nameof(dto.SlotId));

        await using var tx = await _db.Database.BeginTransactionAsync();

        var slot = await _interviewSlotsRepository.GetByIdAsync(dto.SlotId);
        if (slot is null)
            throw new KeyNotFoundException($"Interview slot with id '{dto.SlotId}' was not found.");

        if (slot.SlotStatus != SlotStatus.Unassigned)
            throw new InvalidOperationException("Cannot schedule interview: slot is not available.");

        var existingForSlot = await _scheduledInterviewsRepository.GetBySlotIdAsync(dto.SlotId);
        if (existingForSlot is not null && existingForSlot.InterviewStatus != InterviewStatus.Cancelled)
            throw new InvalidOperationException("Cannot schedule interview: slot already has a scheduled interview.");

        ValidateStartEnd(slot.TimeStart, slot.TimeEnd);

        var studentHasOverlappingScheduled = await _db.ScheduledInterviews
            .Join(
                _db.InterviewSlots,
                si => si.InterviewSlotID,
                s => s.InterviewSlotID,
                (si, s) => new { si, s })
            .AnyAsync(x =>
                x.si.StudentId == dto.StudentId &&
                x.si.InterviewStatus != InterviewStatus.Cancelled &&
                x.s.TimeStart < slot.TimeEnd &&
                x.s.TimeEnd > slot.TimeStart);

        if (studentHasOverlappingScheduled)
            throw new InvalidOperationException("Cannot schedule interview: student already has a scheduled interview overlapping this time window.");

        var hasAvailable = await _studentAvailabilityRepository.CheckIfSlotIsAvailableAsync(
            dto.StudentId,
            slot.TimeStart,
            slot.TimeEnd);

        if (!hasAvailable)
            throw new InvalidOperationException("Cannot schedule interview: student is not available for the requested time window.");

        var hasBlockingUnavailable = await _db.StudentAvailabilities.AnyAsync(sa =>
            sa.StudentId == dto.StudentId &&
            sa.Status == AvailabilityStatus.Unavailable &&
            sa.StartTime < slot.TimeEnd &&
            sa.EndTime > slot.TimeStart);

        if (hasBlockingUnavailable)
            throw new InvalidOperationException("Cannot schedule interview: student has an overlapping unavailable record.");

        var entity = new ScheduledInterview
        {
            StudentId = dto.StudentId,
            InterviewSlotID = slot.InterviewSlotID,
            InterviewStatus = InterviewStatus.Scheduled,
            Comments = dto.Comments
        };

        await _scheduledInterviewsRepository.AddAsync(entity);

        slot.SlotStatus = SlotStatus.Assigned;
        _interviewSlotsRepository.Update(slot);

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        // Ensure slot details are available for mapping (repo includes slot).
        var created = await _scheduledInterviewsRepository.GetByIdAsync(entity.Id);
        if (created is null)
            throw new InvalidOperationException("Scheduled interview was created but could not be reloaded.");

        return _mapper.Map<ScheduledInterviewDto>(created);
    }

    /// <summary>
    /// Gets all scheduled interviews for a specific student (including slot details).
    /// </summary>
    public async Task<List<ScheduledInterviewDto>> GetScheduledInterviewsForStudentAsync(long studentId)
    {
        if (studentId <= 0) throw new ArgumentOutOfRangeException(nameof(studentId));

        var entities = await _db.ScheduledInterviews
            .AsNoTracking()
            .Include(si => si.InterviewSlot)
            .Where(si => si.StudentId == studentId)
            .OrderBy(si => si.InterviewSlot.TimeStart)
            .ToListAsync();

        return entities.Select(e => _mapper.Map<ScheduledInterviewDto>(e)).ToList();
    }

    /// <summary>
    /// Gets all scheduled interviews for a specific slot id (typically zero or one), including slot details.
    /// </summary>
    public async Task<List<ScheduledInterviewDto>> GetScheduledInterviewsForSlotAsync(long slotId)
    {
        if (slotId <= 0) throw new ArgumentOutOfRangeException(nameof(slotId));

        var entities = await _db.ScheduledInterviews
            .AsNoTracking()
            .Include(si => si.InterviewSlot)
            .Where(si => si.InterviewSlotID == slotId)
            .ToListAsync();

        return entities.Select(e => _mapper.Map<ScheduledInterviewDto>(e)).ToList();
    }

    /// <summary>
    /// Cancels a scheduled interview (soft change via status) and frees the associated slot atomically.
    /// </summary>
    public async Task CancelScheduledInterviewAsync(long scheduledInterviewId, string reason)
    {
        if (scheduledInterviewId <= 0) throw new ArgumentOutOfRangeException(nameof(scheduledInterviewId));

        await using var tx = await _db.Database.BeginTransactionAsync();

        var entity = await _db.ScheduledInterviews
            .Include(si => si.InterviewSlot)
            .FirstOrDefaultAsync(si => si.Id == scheduledInterviewId);

        if (entity is null)
            throw new KeyNotFoundException($"Scheduled interview with id '{scheduledInterviewId}' was not found.");

        entity.InterviewStatus = InterviewStatus.Cancelled;

        var normalizedReason = reason?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedReason))
            entity.Comments = normalizedReason;

        if (entity.InterviewSlot is not null)
        {
            entity.InterviewSlot.SlotStatus = SlotStatus.Unassigned;
            _db.InterviewSlots.Update(entity.InterviewSlot);
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    /// <summary>
    /// Updates a scheduled interview status and comments. If status changes to Cancelled, frees the slot.
    /// </summary>
    public async Task UpdateInterviewStatusAsync(long scheduledInterviewId, string newStatus, string comments)
    {
        if (scheduledInterviewId <= 0) throw new ArgumentOutOfRangeException(nameof(scheduledInterviewId));
        if (string.IsNullOrWhiteSpace(newStatus)) throw new ArgumentException("Status is required.", nameof(newStatus));

        if (!Enum.TryParse<InterviewStatus>(newStatus, ignoreCase: true, out var parsed))
            throw new ArgumentException($"Invalid interview status '{newStatus}'.", nameof(newStatus));

        await using var tx = await _db.Database.BeginTransactionAsync();

        var entity = await _db.ScheduledInterviews
            .Include(si => si.InterviewSlot)
            .FirstOrDefaultAsync(si => si.Id == scheduledInterviewId);

        if (entity is null)
            throw new KeyNotFoundException($"Scheduled interview with id '{scheduledInterviewId}' was not found.");

        entity.InterviewStatus = parsed;
        entity.Comments = comments;

        if (parsed == InterviewStatus.Cancelled && entity.InterviewSlot is not null)
        {
            entity.InterviewSlot.SlotStatus = SlotStatus.Unassigned;
            _db.InterviewSlots.Update(entity.InterviewSlot);
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    /// <summary>
    /// Returns students that are available for the requested time window based on student availability records.
    /// A student is considered available when at least one Available record overlaps the range and no Unavailable record overlaps it.
    /// </summary>
    public async Task<List<StudentWithAvailabilityDto>> GetAvailableStudentsBySlotTime(DateTime start, DateTime end)
    {
        ValidateStartEnd(start, end);

        var availableStudentIds = await _db.StudentAvailabilities
            .AsNoTracking()
            .Where(sa =>
                sa.Status == AvailabilityStatus.Available &&
                sa.StartTime < end &&
                sa.EndTime > start)
            .Select(sa => sa.StudentId)
            .Distinct()
            .ToListAsync();

        if (availableStudentIds.Count == 0)
            return new List<StudentWithAvailabilityDto>();

        var blockedStudentIds = await _db.StudentAvailabilities
            .AsNoTracking()
            .Where(sa =>
                availableStudentIds.Contains(sa.StudentId) &&
                sa.Status == AvailabilityStatus.Unavailable &&
                sa.StartTime < end &&
                sa.EndTime > start)
            .Select(sa => sa.StudentId)
            .Distinct()
            .ToListAsync();

        var scheduledOverlappingStudentIds = await _db.ScheduledInterviews
            .AsNoTracking()
            .Join(
                _db.InterviewSlots.AsNoTracking(),
                si => si.InterviewSlotID,
                s => s.InterviewSlotID,
                (si, s) => new { si, s })
            .Where(x =>
                availableStudentIds.Contains(x.si.StudentId) &&
                x.si.InterviewStatus != InterviewStatus.Cancelled &&
                x.s.TimeStart < end &&
                x.s.TimeEnd > start)
            .Select(x => x.si.StudentId)
            .Distinct()
            .ToListAsync();

        var blocked = blockedStudentIds.Count == 0
            ? new HashSet<long>()
            : blockedStudentIds.ToHashSet();

        foreach (var studentId in scheduledOverlappingStudentIds)
            blocked.Add(studentId);

        return availableStudentIds
            .Where(id => !blocked.Contains(id))
            .Select(id => new StudentWithAvailabilityDto
            {
                StudentId = id,
                Start = start,
                End = end
            })
            .ToList();
    }

    private static void ValidateStartEnd(DateTime start, DateTime end)
    {
        if (start >= end)
            throw new ArgumentException("Start must be earlier than End.");
    }
}

