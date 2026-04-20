using AutoMapper;
using SchedulingService.BLL.Repositories;
using SchedulingService.Data;
using SchedulingService.DTOs.StudentAvailability;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.BLL.Services;

public sealed class StudentAvailabilityService
{
    private readonly StudentAvailabilityRepository _studentAvailabilityRepository;
    private readonly SchedulingDbContext _db;
    private readonly IMapper _mapper;

    public StudentAvailabilityService(
        StudentAvailabilityRepository studentAvailabilityRepository,
        SchedulingDbContext db,
        IMapper mapper)
    {
        _studentAvailabilityRepository = studentAvailabilityRepository;
        _db = db;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets availability records for a student. When <paramref name="date"/> is provided,
    /// returns only records that overlap that calendar day (00:00 to next day 00:00, exclusive).
    /// </summary>
    public async Task<IReadOnlyList<StudentAvailability>> GetStudentAvailabilityAsync(
        long studentId,
        DateTime? date = null)
    {
        if (studentId <= 0) throw new ArgumentOutOfRangeException(nameof(studentId));

        if (date is null)
            return await _studentAvailabilityRepository.GetByStudentIdAsync(studentId);

        var dayStart = date.Value.Date;
        var dayEndExclusive = dayStart.AddDays(1);

        return await _studentAvailabilityRepository.GetByStudentIdOverlappingDateRangeAsync(
            studentId,
            dayStart,
            dayEndExclusive);
    }

    /// <summary>
    /// Creates a new availability record for a student after validating the time range and enforcing strict non-overlap.
    /// Persists changes via <see cref="SchedulingDbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>.
    /// </summary>
    public async Task<StudentAvailability> CreateAvailabilityAsync(CreateStudentAvailabilityDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (dto.StudentId <= 0) throw new ArgumentOutOfRangeException(nameof(dto.StudentId));

        ValidateStartEnd(dto.StartTime, dto.EndTime);

        var overlaps = await _studentAvailabilityRepository.GetByStudentIdOverlappingDateRangeAsync(
            dto.StudentId,
            dto.StartTime,
            dto.EndTime);

        if (overlaps.Count > 0)
            throw new InvalidOperationException("Cannot create availability: time range overlaps an existing record.");

        var entity = _mapper.Map<StudentAvailability>(dto);
        await _studentAvailabilityRepository.AddAsync(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Updates an existing availability record by id after validating the time range and enforcing strict non-overlap
    /// (excluding the record being updated). Persists changes via <see cref="SchedulingDbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>.
    /// </summary>
    public async Task<StudentAvailability> UpdateAvailabilityAsync(UpdateStudentAvailabilityDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (dto.Id <= 0) throw new ArgumentOutOfRangeException(nameof(dto.Id));

        var entity = await _studentAvailabilityRepository.GetByIdAsync(dto.Id);
        if (entity is null)
            throw new KeyNotFoundException($"Student availability with id '{dto.Id}' was not found.");

        var start = dto.StartTime ?? entity.StartTime;
        var end = dto.EndTime ?? entity.EndTime;
        ValidateStartEnd(start, end);
        
        var overlaps = await _studentAvailabilityRepository.GetByStudentIdOverlappingDateRangeAsync(
            entity.StudentId,
            start,
            end);

        if (overlaps.Any(sa => sa.Id != dto.Id))
            throw new InvalidOperationException("Cannot update availability: time range overlaps an existing record.");

        if (dto.StartTime.HasValue)
            entity.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue)
            entity.EndTime = dto.EndTime.Value;
        if (dto.Status.HasValue)
            entity.Status = dto.Status.Value;
        if (dto.ReasonStudent is not null)
            entity.ReasonStudent = dto.ReasonStudent;
        
        

        await _db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Sets all availability records overlapping the given day to either Available or Unavailable, and updates the student reason.
    /// If no records exist for that day, creates a full-day record (00:00 to 23:59) and saves it.
    /// </summary>
    public async Task UpdateWholeDayStatusAsync(
        long studentId,
        DateTime date,
        AvailabilityStatus availabilityStatus,
        string reason)
    {
        if (studentId <= 0) throw new ArgumentOutOfRangeException(nameof(studentId));

        var dayStartQuery = date.Date;
        var dayEndExclusiveQuery = dayStartQuery.AddDays(1);

        var dayRecords = await _studentAvailabilityRepository.GetByStudentIdOverlappingDateRangeAsync(
            studentId,
            dayStartQuery,
            dayEndExclusiveQuery);

        if (dayRecords.Count == 0)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);

            var entity = new StudentAvailability(
                id: 0,
                studentId: studentId,
                startTime: dayStart,
                endTime: dayEnd,
                status: availabilityStatus,
                reasonStudent: reason ?? string.Empty,
                reasonStatus: AvailabilityReasonKind.Personal);

            await _studentAvailabilityRepository.AddAsync(entity);
            await _db.SaveChangesAsync();
            return;
        }

        var normalizedReason = reason ?? string.Empty;

        foreach (var rec in dayRecords)
        {
            rec.Status = availabilityStatus;
            rec.ReasonStudent = normalizedReason;
        }

        await _db.SaveChangesAsync();
    }

    public Task MarkWholeDayAsOccupiedAsync(long studentId, DateTime date)
    {
        if (studentId <= 0) throw new ArgumentOutOfRangeException(nameof(studentId));
        return UpdateWholeDayStatusAsync(
            studentId: studentId,
            date: date,
            availabilityStatus: AvailabilityStatus.Unavailable,
            reason: string.Empty);
    }

    public Task MarkWholeDayAsFreeAsync(long studentId, DateTime date)
    {
        if (studentId <= 0) throw new ArgumentOutOfRangeException(nameof(studentId));
        return UpdateWholeDayStatusAsync(
            studentId: studentId,
            date: date,
            availabilityStatus: AvailabilityStatus.Available,
            reason: string.Empty);
    }

    public async Task BulkUpdateStatusByDateRangeAsync(
        long studentId,
        DateTime start,
        DateTime end,
        AvailabilityStatus status,
        string reason)
    {
        if (studentId <= 0) throw new ArgumentOutOfRangeException(nameof(studentId));

        var startDate = start.Date;
        var endDate = end.Date;
        if (endDate < startDate)
            throw new ArgumentException("End date must be on or after start date.");

        var newStatus = status;
        var normalizedReason = reason ?? string.Empty;

        var rangeEndExclusive = endDate.AddDays(1);

        var records = await _studentAvailabilityRepository.GetByStudentIdOverlappingDateRangeAsync(
            studentId,
            startDate,
            rangeEndExclusive);

        foreach (var rec in records)
        {
            rec.Status = newStatus;
            rec.ReasonStudent = normalizedReason;
        }

        // Ensure the range is fully represented: if a day has no records at all, create a full-day record.
        var missingDays = new List<StudentAvailability>();
        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            var dayStart = day;
            var dayEndExclusive = dayStart.AddDays(1);

            var hasAny = records.Any(r => r.StartTime < dayEndExclusive && r.EndTime > dayStart);
            if (hasAny) continue;

            var dayEnd = dayEndExclusive.AddTicks(-1);
            missingDays.Add(new StudentAvailability(
                id: 0,
                studentId: studentId,
                startTime: dayStart,
                endTime: dayEnd,
                status: newStatus,
                reasonStudent: normalizedReason,
                reasonStatus: AvailabilityReasonKind.Personal));
        }

        if (missingDays.Count > 0)
            await _studentAvailabilityRepository.AddRangeAsync(missingDays);

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Checks whether a student has any overlapping availability marked as Available for the given time range.
    /// </summary>
    public Task<bool> CheckAvailabilityForSchedulingAsync(
        long studentId,
        DateTime start,
        DateTime end)
    {
        if (studentId <= 0) throw new ArgumentOutOfRangeException(nameof(studentId));
        ValidateStartEnd(start, end);

        return _studentAvailabilityRepository.CheckIfSlotIsAvailableAsync(studentId, start, end);
    }

    /// <summary>
    /// Deletes an availability record by id (throws if not found) and persists the change.
    /// </summary>
    public async Task DeleteAvailabilityAsync(long id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

        var entity = await _studentAvailabilityRepository.GetByIdAsync(id);
        if (entity is null)
            throw new KeyNotFoundException($"Student availability with id '{id}' was not found.");

        _studentAvailabilityRepository.Remove(entity);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Validates that <paramref name="start"/> is strictly earlier than <paramref name="end"/>.
    /// </summary>
    private static void ValidateStartEnd(DateTime start, DateTime end)
    {
        if (start >= end)
            throw new ArgumentException("StartTime must be earlier than EndTime.");
    }

 
}

