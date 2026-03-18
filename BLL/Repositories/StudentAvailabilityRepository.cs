
using Microsoft.EntityFrameworkCore;
using SchedulingService.Data;
using SchedulingService.Models;
using SchedulingService.Enums;

namespace SchedulingService.BLL.Repositories;

/// <summary>
/// Repository for CRUD and query operations on <see cref="StudentAvailability"/> entities.
/// Provides filtering by student, date ranges, and availability status.
/// </summary>
public class StudentAvailabilityRepository : IRepository<StudentAvailability>
{
    private readonly SchedulingDbContext _db;

    public StudentAvailabilityRepository(SchedulingDbContext db)
    {
        _db = db;
    }

<<<<<<< HEAD
=======
    /// <inheritdoc />
>>>>>>> 22bf93010b42e0fabde7a0681a25b2fb84fdeb6b
    public Task<StudentAvailability?> GetByIdAsync(long id) =>
        _db.StudentAvailabilities.FindAsync(id).AsTask();

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudentAvailability>> GetAllAsync() =>
        await _db.StudentAvailabilities.ToListAsync();

    /// <inheritdoc />
    public async Task AddAsync(StudentAvailability entity) =>
        await _db.StudentAvailabilities.AddAsync(entity);

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<StudentAvailability> entities) =>
        await _db.StudentAvailabilities.AddRangeAsync(entities);

    /// <summary>
    /// Returns all availability records for the given student.
    /// </summary>
    public async Task<IReadOnlyList<StudentAvailability>> GetByStudentIdAsync(long studentId) =>
        await _db.StudentAvailabilities
            .Where(sa => sa.StudentId == studentId)
            .ToListAsync();

    /// <summary>
    /// Returns availability records fully contained within the given date range (StartTime &gt;= start, EndTime &lt;= end).
    /// </summary>
    public async Task<IReadOnlyList<StudentAvailability>> GetByStudentIdAndDateRangeAsync(
        long studentId,
        DateTime start,
        DateTime end) =>
        await _db.StudentAvailabilities
            .Where(sa => sa.StudentId == studentId &&
                         sa.StartTime >= start &&
                         sa.EndTime <= end)
            .ToListAsync();

    /// <summary>
    /// Returns availability records that overlap the given date range (any overlap with [start, end]).
    /// </summary>
    public async Task<IReadOnlyList<StudentAvailability>> GetByStudentIdOverlappingDateRangeAsync(
        long studentId,
        DateTime start,
        DateTime end) =>
        await _db.StudentAvailabilities
            .Where(sa => sa.StudentId == studentId &&
                         sa.StartTime < end &&
                         sa.EndTime > start)
            .ToListAsync();

    /// <summary>
    /// Returns availability records for the given student on the given date (by StartTime.Date).
    /// </summary>
    public async Task<IReadOnlyList<StudentAvailability>> GetSpecificStudentAvailabilityForDayAsync(
        long studentId,
        DateTime date) =>
        await _db.StudentAvailabilities
            .Where(sa => sa.StudentId == studentId &&
                         sa.StartTime.Date == date.Date)
            .ToListAsync();

    /// <summary>
    /// Checks whether the student has any availability with status Available that overlaps [startTime, endTime].
    /// </summary>
    public async Task<bool> CheckIfSlotIsAvailableAsync(
        long studentId,
        DateTime startTime,
        DateTime endTime) =>
        await _db.StudentAvailabilities
            .AnyAsync(sa =>
                sa.StudentId == studentId &&
                sa.Status == AvailabilityStatus.Available &&
                sa.StartTime < endTime &&
                sa.EndTime > startTime);

    /// <inheritdoc />
    public void Update(StudentAvailability entity) =>
        _db.StudentAvailabilities.Update(entity);

    /// <inheritdoc />
    public void Remove(StudentAvailability entity) =>
        _db.StudentAvailabilities.Remove(entity);

    /// <inheritdoc />
    public void RemoveRange(IEnumerable<StudentAvailability> entities) =>
        _db.StudentAvailabilities.RemoveRange(entities);

    /// <inheritdoc />
    public IQueryable<StudentAvailability> Query() =>
        _db.StudentAvailabilities.AsQueryable();
}
