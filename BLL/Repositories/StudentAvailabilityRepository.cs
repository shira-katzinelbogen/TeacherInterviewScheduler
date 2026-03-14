using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulingService.Data;
using SchedulingService.Models;


namespace SchedulingService.BLL.Repositories;

public class StudentAvailabilityRepository : IRepository<StudentAvailability>
{
    private readonly SchedulingDbContext _db;

    public StudentAvailabilityRepository(SchedulingDbContext db)
    {
        _db = db;
    }

    public Task<StudentAvailability?> GetByIdAsync(int id) =>
        _db.StudentAvailabilities.FindAsync(id).AsTask();

    public async Task<IReadOnlyList<StudentAvailability>> GetAllAsync() =>
        await _db.StudentAvailabilities.ToListAsync();

    public async Task AddAsync(StudentAvailability entity) =>
        await _db.StudentAvailabilities.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<StudentAvailability> entities) =>
        await _db.StudentAvailabilities.AddRangeAsync(entities);

    public void Update(StudentAvailability entity) =>
        _db.StudentAvailabilities.Update(entity);

    public void Remove(StudentAvailability entity) =>
        _db.StudentAvailabilities.Remove(entity);

    public void RemoveRange(IEnumerable<StudentAvailability> entities) =>
        _db.StudentAvailabilities.RemoveRange(entities);

    public IQueryable<StudentAvailability> Query() =>
        _db.StudentAvailabilities.AsQueryable();
}
