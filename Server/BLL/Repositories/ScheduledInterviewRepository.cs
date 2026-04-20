using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulingService.Data;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.BLL.Repositories;

public class ScheduledInterviewsRepository : IRepository<ScheduledInterview>
{
    private readonly SchedulingDbContext _db;

    public ScheduledInterviewsRepository(SchedulingDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets a scheduled interview by its primary key id (including the related interview slot).
    /// </summary>
    public async Task<ScheduledInterview?> GetByIdAsync(long id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

        return await _db.ScheduledInterviews
            .AsNoTracking()
            .Include(si => si.InterviewSlot)
            .FirstOrDefaultAsync(si => si.Id == id);
    }

    /// <summary>
    /// Gets the scheduled interview assigned to the specified interview slot id (including the related interview slot).
    /// </summary>
    public async Task<ScheduledInterview?> GetBySlotIdAsync(long slotId)
    {
        if (slotId <= 0) throw new ArgumentOutOfRangeException(nameof(slotId));

        return await _db.ScheduledInterviews
            .AsNoTracking()
            .Include(si => si.InterviewSlot)
            .FirstOrDefaultAsync(si => si.InterviewSlotID == slotId);
    }

    /// <summary>
    /// Gets all scheduled interviews for the specified student id (including related interview slots).
    /// </summary>
    public async Task<IEnumerable<ScheduledInterview>> GetByStudentIdAsync(long studentId)
    {
        if (studentId <= 0) throw new ArgumentOutOfRangeException(nameof(studentId));

        return await _db.ScheduledInterviews
            .AsNoTracking()
            .Include(si => si.InterviewSlot)
            .Where(si => si.StudentId == studentId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all scheduled interviews relevant to the current teacher/job context (including related interview slots).
    /// </summary>
    public async Task<IEnumerable<ScheduledInterview>> GetScheduledForTeacherAsync()
    {
        // No teacher context exists in this project yet (no TeacherId/claims access in BLL).
        // Return all scheduled interviews with their slot details.
        return await _db.ScheduledInterviews
            .AsNoTracking()
            .Include(si => si.InterviewSlot)
            .OrderBy(si => si.InterviewSlot.TimeStart)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new scheduled interview to the context. Caller is responsible for saving changes.
    /// </summary>
    public async Task AddAsync(ScheduledInterview entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        await _db.ScheduledInterviews.AddAsync(entity);
    }

    /// <summary>
    /// Updates the status (and comments) for an existing scheduled interview. Caller is responsible for saving changes.
    /// </summary>
    public async Task UpdateStatusAsync(long id, string newStatus, string comments)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(newStatus)) throw new ArgumentException("Status is required.", nameof(newStatus));

        if (!Enum.TryParse<InterviewStatus>(newStatus, ignoreCase: true, out var parsedStatus))
            throw new ArgumentException($"Invalid interview status '{newStatus}'.", nameof(newStatus));

        var entity = await _db.ScheduledInterviews.FirstOrDefaultAsync(si => si.Id == id);
        if (entity is null)
            throw new KeyNotFoundException($"Scheduled interview with id '{id}' was not found.");

        entity.InterviewStatus = parsedStatus;
        entity.Comments = comments;
    }

    /// <summary>
    /// Cancels a scheduled interview by setting its status to <see cref="InterviewStatus.Cancelled"/>. Caller is responsible for saving changes.
    /// </summary>
    public async Task CancelInterviewAsync(long id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

        var entity = await _db.ScheduledInterviews.FirstOrDefaultAsync(si => si.Id == id);
        if (entity is null)
            throw new KeyNotFoundException($"Scheduled interview with id '{id}' was not found.");

        entity.InterviewStatus = InterviewStatus.Cancelled;
        entity.Comments ??= "Cancelled";
    }

    /// <summary>
    /// Deletes a scheduled interview by id. Caller is responsible for saving changes.
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

        var entity = await _db.ScheduledInterviews.FirstOrDefaultAsync(si => si.Id == id);
        if (entity is null)
            return;

        _db.ScheduledInterviews.Remove(entity);
    }

    Task<ScheduledInterview?> IRepository<ScheduledInterview>.GetByIdAsync(long id) =>
        GetByIdAsync(id);

    async Task<IReadOnlyList<ScheduledInterview>> IRepository<ScheduledInterview>.GetAllAsync() =>
        await _db.ScheduledInterviews
            .AsNoTracking()
            .Include(si => si.InterviewSlot)
            .OrderBy(si => si.InterviewSlot.TimeStart)
            .ToListAsync();

    Task IRepository<ScheduledInterview>.AddRangeAsync(IEnumerable<ScheduledInterview> entities)
    {
        if (entities is null) throw new ArgumentNullException(nameof(entities));
        return _db.ScheduledInterviews.AddRangeAsync(entities);
    }

    void IRepository<ScheduledInterview>.Update(ScheduledInterview entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        _db.ScheduledInterviews.Update(entity);
    }

    void IRepository<ScheduledInterview>.Remove(ScheduledInterview entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        _db.ScheduledInterviews.Remove(entity);
    }

    void IRepository<ScheduledInterview>.RemoveRange(IEnumerable<ScheduledInterview> entities)
    {
        if (entities is null) throw new ArgumentNullException(nameof(entities));
        _db.ScheduledInterviews.RemoveRange(entities);
    }

    IQueryable<ScheduledInterview> IRepository<ScheduledInterview>.Query() =>
        _db.ScheduledInterviews.AsQueryable();
}
