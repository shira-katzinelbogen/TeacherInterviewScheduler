using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulingService.Data;
using SchedulingService.Models;


namespace SchedulingService.BLL.Repositories;

public class ScheduledInterviewRepository : IRepository<ScheduledInterview>
{
    private readonly SchedulingDbContext _db;

    public ScheduledInterviewRepository(SchedulingDbContext db)
    {
        _db = db;
    }

    public Task<ScheduledInterview?> GetByIdAsync(long id) =>
        _db.ScheduledInterviews.FindAsync(id).AsTask();

    public async Task<IReadOnlyList<ScheduledInterview>> GetAllAsync() =>
        await _db.ScheduledInterviews.ToListAsync();

    public async Task AddAsync(ScheduledInterview entity) =>
        await _db.ScheduledInterviews.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<ScheduledInterview> entities) =>
        await _db.ScheduledInterviews.AddRangeAsync(entities);

    public void Update(ScheduledInterview entity) =>
        _db.ScheduledInterviews.Update(entity);

    public void Remove(ScheduledInterview entity) =>
        _db.ScheduledInterviews.Remove(entity);

    public void RemoveRange(IEnumerable<ScheduledInterview> entities) =>
        _db.ScheduledInterviews.RemoveRange(entities);

    public IQueryable<ScheduledInterview> Query() =>
        _db.ScheduledInterviews.AsQueryable();
}
