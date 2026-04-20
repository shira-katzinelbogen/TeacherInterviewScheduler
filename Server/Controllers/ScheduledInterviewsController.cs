using Microsoft.AspNetCore.Mvc;
using SchedulingService.BLL.Services;
using SchedulingService.DTOs.ScheduledInterviews;

namespace SchedulingService.Controllers;

[ApiController]
[Route("api/scheduled-interviews")]
public sealed class ScheduledInterviewsController : ControllerBase
{
    private readonly ScheduleInterviewsService _service;

    public ScheduledInterviewsController(ScheduleInterviewsService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ScheduledInterviewDto>> Schedule([FromBody] ScheduleInterviewDto dto)
    {
        if (dto is null)
            return BadRequest("Request body is required.");

        try
        {
            var result = await _service.ScheduleInterviewAsync(dto);
            return CreatedAtAction(nameof(GetForStudent), new { studentId = result.StudentId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("~/api/students/{studentId:long}/scheduled-interviews")]
    public async Task<ActionResult<List<ScheduledInterviewDto>>> GetForStudent([FromRoute] long studentId)
    {
        try
        {
            var items = await _service.GetScheduledInterviewsForStudentAsync(studentId);
            return Ok(items);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("~/api/interview-slots/{slotId:long}/scheduled-interviews")]
    public async Task<ActionResult<List<ScheduledInterviewDto>>> GetForSlot([FromRoute] long slotId)
    {
        try
        {
            var items = await _service.GetScheduledInterviewsForSlotAsync(slotId);
            return Ok(items);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{scheduledInterviewId:long}/cancel")]
    public async Task<IActionResult> Cancel(
        [FromRoute] long scheduledInterviewId,
        [FromBody] CancelScheduledInterviewRequest request)
    {
        try
        {
            await _service.CancelScheduledInterviewAsync(
                scheduledInterviewId,
                request?.Reason ?? string.Empty);
            return NoContent();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{scheduledInterviewId:long}/status")]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] long scheduledInterviewId,
        [FromBody] UpdateInterviewStatusRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        try
        {
            await _service.UpdateInterviewStatusAsync(
                scheduledInterviewId,
                request.NewStatus ?? string.Empty,
                request.Comments ?? string.Empty);
            return NoContent();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("available-students")]
    public async Task<ActionResult<List<StudentWithAvailabilityDto>>> GetAvailableStudents(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end) =>
        Ok(await _service.GetAvailableStudentsBySlotTime(start, end));

    public sealed class CancelScheduledInterviewRequest
    {
        public string? Reason { get; set; }
    }

    public sealed class UpdateInterviewStatusRequest
    {
        public string? NewStatus { get; set; }
        public string? Comments { get; set; }
    }
}

