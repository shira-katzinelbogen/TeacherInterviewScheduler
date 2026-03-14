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

    public async Task<InterviewSlots?> GetByIdAsync(int id)
    {
        return await _db.InterviewSlots
            .FirstOrDefaultAsync(s => s.InterviewSlotID == id);
    }

    public async Task<IReadOnlyList<InterviewSlots>> GetAllAsync()
    {
        return await _db.InterviewSlots.ToListAsync();
    }

    public async Task AddAsync(InterviewSlots interviewSlot)
    {
        await _db.InterviewSlots.AddAsync(interviewSlot);
        await _db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<InterviewSlots> interviewSlots)
    {
        await _db.InterviewSlots.AddRangeAsync(interviewSlots);
        await _db.SaveChangesAsync();
    }

    public void Update(InterviewSlots interviewSlot)
    {
        _db.InterviewSlots.Update(interviewSlot);
    }

    public void Remove(InterviewSlots interviewSlot)
    {
        _db.InterviewSlots.Remove(interviewSlot);
    }

    public void RemoveRange(IEnumerable<InterviewSlots> interviewSlots)
    {
        _db.InterviewSlots.RemoveRange(interviewSlots);
    }

    public IQueryable<InterviewSlots> Query()
    {
        return _db.InterviewSlots.AsQueryable();
    }

    /// <summary>Get slots by <see cref="SlotStatus"/> (Unassigned / Assigned).</summary>
    public async Task<IReadOnlyList<InterviewSlots>> GetByStatusAsync(SlotStatus slotStatus)
    {
        return await _db.InterviewSlots
            .Where(s => s.SlotStatus == slotStatus)
            .ToListAsync();
    }

    /// <summary>Get slots by <see cref="InterviewType"/> (Technical, Professional, Personal, Other).</summary>
    public async Task<IReadOnlyList<InterviewSlots>> GetByInterviewTypeAsync(InterviewType interviewType)
    {
        return await _db.InterviewSlots
            .Where(s => s.InterviewType == interviewType)
            .ToListAsync();
    }
}
