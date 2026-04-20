using Microsoft.AspNetCore.Mvc;
using SchedulingService.BLL.Services;
using SchedulingService.DTOs.StudentAvailability;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.Controllers;

[ApiController]
[Route("api/students/{studentId:long}/availability")]
public sealed class StudentAvailabilityController : ControllerBase
{
    private readonly StudentAvailabilityService _service;

    public StudentAvailabilityController(StudentAvailabilityService service)
    {
        _service = service;
    }

    // Authorized roles: Teacher, Student
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentAvailability>>> Get(
        [FromRoute] long studentId,
        [FromQuery] DateTime? date = null)
    {
        var items = await _service.GetStudentAvailabilityAsync(studentId, date);
        return Ok(items);
    }

    // Authorized roles: Student
    [HttpPost]
    public async Task<ActionResult<StudentAvailability>> Create(
        [FromRoute] long studentId,
        [FromBody] CreateStudentAvailabilityDto dto)
    {
        if (dto is null) return BadRequest();
        if (dto.StudentId != 0 && dto.StudentId != studentId)
            return BadRequest("StudentId in body must match route.");

        dto.StudentId = studentId;
        var created = await _service.CreateAvailabilityAsync(dto);
        return CreatedAtAction(nameof(Get), new { studentId, date = (DateTime?)null }, created);
    }

    // Authorized roles: Student
    [HttpPut("{id:long}")]
    public async Task<ActionResult<StudentAvailability>> Update(
        [FromRoute] long studentId,
        [FromRoute] long id,
        [FromBody] UpdateStudentAvailabilityDto dto)
    {
        if (dto is null) return BadRequest();
        dto.Id = id;

        // Note: service currently validates by id and uses the existing entity's StudentId;
        // studentId route is kept for RESTful scoping.
        var updated = await _service.UpdateAvailabilityAsync(dto);
        if (updated.StudentId != studentId)
            return NotFound();

        return Ok(updated);
    }

    // Authorized roles: Student
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete([FromRoute] long studentId, [FromRoute] long id)
    {
        // Ensure the record belongs to the student (avoid deleting cross-student by id).
        var items = await _service.GetStudentAvailabilityAsync(studentId);
        if (items.All(x => x.Id != id))
            return NotFound();

        await _service.DeleteAvailabilityAsync(id);
        return NoContent();
    }

    // Authorized roles: Student
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkUpdate(
        [FromRoute] long studentId,
        [FromBody] BulkUpdateStudentAvailabilityDto dto)
    {
        if (dto is null) return BadRequest();

        await _service.BulkUpdateStatusByDateRangeAsync(
            studentId: studentId,
            start: dto.Start,
            end: dto.End,
            status: dto.Status,
            reason: dto.Reason ?? string.Empty);

        return NoContent();
    }

    // Authorized roles: Student
    [HttpPost("day/{date:datetime}/status")]
    public Task UpdateWholeDayStatus(
        [FromRoute] long studentId,
        [FromRoute] DateTime date,
        [FromQuery] AvailabilityStatus status,
        [FromQuery] string? reason = null) =>
        _service.UpdateWholeDayStatusAsync(studentId, date, status, reason ?? string.Empty);
}

