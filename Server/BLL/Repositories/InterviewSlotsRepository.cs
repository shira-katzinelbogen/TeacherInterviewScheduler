using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulingService.Data;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.BLL.Repositories;

public class InterviewSlotsRepository : IRepository<InterviewSlots>
{
    private readonly SchedulingDbContext _db;

    public InterviewSlotsRepository(SchedulingDbContext db)
    {
        _db = db;
    }

    public async Task<InterviewSlots?> GetByIdAsync(long id)
    {
        return await _db.InterviewSlots
            .FirstOrDefaultAsync(s => s.InterviewSlotID == id);
    }

    public async Task<IReadOnlyList<InterviewSlots>> GetAllAsync() =>
        await _db.InterviewSlots.ToListAsync();

    public async Task AddAsync(InterviewSlots entity) =>
        await _db.InterviewSlots.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<InterviewSlots> entities) =>
        await _db.InterviewSlots.AddRangeAsync(entities);

    public void Update(InterviewSlots entity) =>
        _db.InterviewSlots.Update(entity);

    public void Remove(InterviewSlots entity) =>
        _db.InterviewSlots.Remove(entity);

    public void RemoveRange(IEnumerable<InterviewSlots> entities) =>
        _db.InterviewSlots.RemoveRange(entities);

    public IQueryable<InterviewSlots> Query() =>
        _db.InterviewSlots.AsQueryable();

    /// <summary>
    /// Get all slots for a specific job.
    /// </summary>
    public async Task<IReadOnlyList<InterviewSlots>> GetByJobIdAsync(long jobId) =>
        await _db.InterviewSlots
            .Where(s => s.JobID == jobId)
            .OrderBy(s => s.TimeStart)
            .ToListAsync();

    /// <summary>
    /// Get free (unassigned) slots for a specific job on a specific date.
    /// </summary>
    public async Task<IReadOnlyList<InterviewSlots>> GetAvailableSlotsByJobAndDateAsync(long jobId, DateTime date) =>
        await _db.InterviewSlots
            .Where(s =>
                s.JobID == jobId &&
                s.SlotStatus == SlotStatus.Unassigned &&
                s.TimeStart.Date == date.Date)
            .OrderBy(s => s.TimeStart)
            .ToListAsync();

    /// <summary>
    /// Get slots whose time window is fully inside the given date-time range.
    /// </summary>
    public async Task<IReadOnlyList<InterviewSlots>> GetSlotsByDateRangeAsync(DateTime start, DateTime end) =>
        await _db.InterviewSlots
            .Where(s => s.TimeStart >= start && s.TimeEnd <= end)
            .OrderBy(s => s.TimeStart)
            .ToListAsync();

    /// <summary>
    /// Create a single free slot.
    /// </summary>
    public async Task CreateSingleSlotAsync(InterviewSlots entity) =>
        await _db.InterviewSlots.AddAsync(entity);

    /// <summary>
    /// Update an existing slot.
    /// </summary>
    public Task UpdateSlotAsync(InterviewSlots entity)
    {
        _db.InterviewSlots.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Update only the status of a slot.
    /// Returns false when the slot is not found or status string is invalid.
    /// </summary>
    public async Task<bool> UpdateSlotStatusAsync(long id, string newStatus)
    {
        var slot = await GetByIdAsync(id);
        if (slot is null)
        {
            return false;
        }

        if (!Enum.TryParse<SlotStatus>(newStatus, ignoreCase: true, out var parsedStatus))
        {
            return false;
        }

        slot.SlotStatus = parsedStatus;
        _db.InterviewSlots.Update(slot);
        return true;
    }

    /// <summary>
    /// Delete a slot by id.
    /// Returns false if the slot was not found.
    /// </summary>
    public async Task<bool> DeleteSlotAsync(long id)
    {
        var slot = await GetByIdAsync(id);
        if (slot is null)
        {
            return false;
        }

        _db.InterviewSlots.Remove(slot);
        return true;
    }

    /// <summary>
    /// Check if a slot is free (unassigned and without scheduled interviews).
    /// </summary>
    public async Task<bool> CheckIfSlotIsFreeAsync(long slotId)
    {
        var slot = await GetByIdAsync(slotId);
        if (slot is null || slot.SlotStatus != SlotStatus.Unassigned)
        {
            return false;
        }

        var hasScheduled = await _db.ScheduledInterviews
            .AnyAsync(si => si.InterviewSlotID == slotId);

        return !hasScheduled;
    }
}
