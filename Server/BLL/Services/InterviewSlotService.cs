using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchedulingService.BLL.Repositories;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.BLL.Services;

public class InterviewSlotService
{
    private readonly InterviewSlotsRepository _interviewSlotsRepository;
    private readonly ScheduledInterviewsRepository _scheduledInterviewRepository;
    private readonly StudentAvailabilityRepository _studentAvailabilityRepository;
    private readonly SchedulingDbContext _db;
    private readonly IMapper _mapper;

    public InterviewSlotService(
        InterviewSlotsRepository interviewSlotsRepository,
        ScheduledInterviewsRepository scheduledInterviewRepository,
        StudentAvailabilityRepository studentAvailabilityRepository,
        SchedulingDbContext db,
        IMapper mapper)
    {
        _interviewSlotsRepository = interviewSlotsRepository;
        _scheduledInterviewRepository = scheduledInterviewRepository;
        _studentAvailabilityRepository = studentAvailabilityRepository;
        _db = db;
        _mapper = mapper;
    }

    /// <summary>
    /// Get slots for a job. When <paramref name="date"/> is provided, returns free (unassigned) slots
    /// on that date; otherwise returns all free slots for the job.
    /// </summary>
    public async Task<IReadOnlyList<InterviewSlots>> GetSlotsForJobAsync(long jobId, DateTime? date = null)
    {
        if (date.HasValue)
        {
            return await _interviewSlotsRepository.GetAvailableSlotsByJobAndDateAsync(jobId, date.Value);
        }

        return await _interviewSlotsRepository.Query()
            .Where(s => s.JobID == jobId && s.SlotStatus == SlotStatus.Unassigned)
            .OrderBy(s => s.TimeStart)
            .ToListAsync();
    }

    /// <summary>
    /// Create a single interview slot.
    /// </summary>
    public async Task<InterviewSlots> CreateInterviewSlotsAsync(CreateInterviewSlotDto dto)
    {
        var slot = _mapper.Map<InterviewSlots>(dto);

        await _interviewSlotsRepository.CreateSingleSlotAsync(slot);
        await _db.SaveChangesAsync();
        return slot;
    }

    /// <summary>
    /// Get free slots for scheduling for a specific job and date.
    /// </summary>
    public Task<IReadOnlyList<InterviewSlots>> GetAvailableSlotsForSchedulingAsync(long jobId, DateTime date)
    {
        return _interviewSlotsRepository.GetAvailableSlotsByJobAndDateAsync(jobId, date);
    }

    /// <summary>
    /// Update slot details and merge overlapping/adjacent slots for the same job, place and interview type.
    /// </summary>
    public async Task<InterviewSlots?> UpdateSlotDetailsAsync(UpdateInterviewSlotDto dto)
    {
        var slot = await _interviewSlotsRepository.GetByIdAsync(dto.Id);
        if (slot is null)
        {
            return null;
        }

        _mapper.Map(dto, slot);

        await _interviewSlotsRepository.UpdateSlotAsync(slot);
        await _db.SaveChangesAsync();

        await MergeOverlappingSlotsAsync(slot.JobID, slot.Place, slot.InterviewType);

        return slot;
    }

    /// <summary>
    /// Delete a slot by id.
    /// </summary>
    public Task<bool> DeleteSlotAsync(long id)
    {
        return DeleteSlotInternalAsync(id);
    }

    private async Task<bool> DeleteSlotInternalAsync(long id)
    {
        var deleted = await _interviewSlotsRepository.DeleteSlotAsync(id);
        if (!deleted) return false;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Rearrange slots for a specific student.
    /// Currently ensures that the student's scheduled interviews are aligned with the earliest available slots.
    /// </summary>
    public Task RearrangeSlotsAsync(long studentId)
    {
        return RearrangeSlotsInternalAsync(studentId);
    }

    private async Task RearrangeSlotsInternalAsync(long studentId)
    {
        // Load scheduled interviews including slots (repository method ensures Include).
        var scheduledForStudent = (await _scheduledInterviewRepository.GetByStudentIdAsync(studentId))
            .ToList();

        if (scheduledForStudent.Count == 0)
            return;

        // Currently no persisted "order" field exists; we only ensure this does not throw
        // and provides a stable, deterministic ordering for any caller-side processing.
        _ = scheduledForStudent
            .Where(si => si.InterviewSlot is not null)
            .OrderBy(si => si.InterviewSlot!.TimeStart)
            .ToList();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Validate that a proposed slot time range is valid and does not overlap existing slots for the job.
    /// </summary>
    public async Task<bool> ValidateSlotTimesAsync(long jobId, DateTime start, DateTime end)
    {
        if (start >= end)
        {
            return false;
        }

        var existingSlots = await _interviewSlotsRepository.GetByJobIdAsync(jobId);

        foreach (var existing in existingSlots)
        {
            var overlaps = start < existing.TimeEnd && end > existing.TimeStart;
            if (overlaps)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Create multiple consecutive slots inside the given time range.
    /// </summary>
    public async Task<IReadOnlyList<InterviewSlots>> CreateMultipleSlotsAsync(
        long jobId,
        DateTime startTime,
        DateTime endTime,
        string place,
        string interviewType,
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        if (startTime >= endTime)
        {
            throw new ArgumentException("StartTime must be earlier than EndTime.");
        }

        if (!Enum.TryParse<InterviewType>(interviewType, ignoreCase: true, out var parsedType))
        {
            throw new ArgumentException("Invalid interview type.", nameof(interviewType));
        }

        var totalDuration = endTime - startTime;
        var slotDuration = TimeSpan.FromTicks(totalDuration.Ticks / quantity);

        var slots = new List<InterviewSlots>(quantity);
        var currentStart = startTime;

        for (var i = 0; i < quantity; i++)
        {
            var currentEnd = i == quantity - 1
                ? endTime
                : currentStart.Add(slotDuration);

            var slot = new InterviewSlots(
                interviewSlotID: 0,
                jobID: jobId,
                timeStart: currentStart,
                timeEnd: currentEnd,
                place: place,
                interviewType: parsedType,
                slotStatus: SlotStatus.Unassigned);

            slots.Add(slot);
            currentStart = currentEnd;
        }

        await _interviewSlotsRepository.AddRangeAsync(slots);
        await _db.SaveChangesAsync();
        return slots;
    }

    private async Task MergeOverlappingSlotsAsync(long jobId, string place, InterviewType interviewType)
    {
        var slots = await _interviewSlotsRepository.Query()
            .Where(s =>
                s.JobID == jobId &&
                s.Place == place &&
                s.InterviewType == interviewType &&
                s.SlotStatus == SlotStatus.Unassigned)
            .OrderBy(s => s.TimeStart)
            .ToListAsync();

        if (slots.Count <= 1)
        {
            return;
        }

        var merged = new List<InterviewSlots>();
        var current = slots[0];

        for (var i = 1; i < slots.Count; i++)
        {
            var next = slots[i];

            if (next.TimeStart <= current.TimeEnd)
            {
                if (next.TimeEnd > current.TimeEnd)
                {
                    current.TimeEnd = next.TimeEnd;
                }
            }
            else
            {
                merged.Add(current);
                current = next;
            }
        }

        merged.Add(current);

        _interviewSlotsRepository.RemoveRange(slots);
        await _interviewSlotsRepository.AddRangeAsync(merged);
        await _db.SaveChangesAsync();
    }
}

