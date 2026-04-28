using Microsoft.AspNetCore.Mvc;
using SchedulingService.BLL.Services;
using SchedulingService.DTOs.InterviewSlots;
using SchedulingService.Models;

namespace SchedulingService.Controllers;

[ApiController]
[Route("api/interview-slots")]
public sealed class InterviewSlotsController : ControllerBase
{
    private readonly InterviewSlotService _service;

    public InterviewSlotsController(
        InterviewSlotService service)
    {
        _service = service;
    }

    /// <summary>
    /// Provides CRUD operations for interview slots.
    /// </summary>
    /// <remarks>
    /// This controller exposes endpoints to create, read, update and delete interview slots.
    /// All business logic is delegated to <see cref="InterviewSlotService"/>.
    /// </remarks>
    // Authorized roles: Teacher
    /// <summary>
    /// Get a single interview slot by its identifier.
    /// </summary>
    /// <param name="id">The interview slot identifier.</param>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<InterviewSlots>> GetById([FromRoute] long id)
    {
        var slot = await _service.GetByIdAsync(id);
        if (slot is null)
        {
            return NotFound();
        }

        return Ok(slot);
    }

    // Authorized roles: Teacher
    /// <summary>
    /// Get all interview slots.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InterviewSlots>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    // Authorized roles: Teacher
    /// <summary>
    /// Create a new interview slot.
    /// </summary>
    /// <param name="dto">The interview slot creation data.</param>
    [HttpPost]
    public async Task<ActionResult<InterviewSlots>> Create([FromBody] CreateInterviewSlotDto dto)
    {
        if (dto is null)
        {
            return BadRequest("Request body is required.");
        }

        if (dto.TimeEnd <= dto.TimeStart)
        {
            return BadRequest("TimeEnd must be later than TimeStart.");
        }

        try
        {
            var isValidRange = await _service.ValidateSlotTimesAsync(
                dto.JobId,
                dto.TimeStart,
                dto.TimeEnd);

            if (!isValidRange)
            {
                return BadRequest("The specified time range is invalid or overlaps existing slots.");
            }

            var created = await _service.CreateInterviewSlotsAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.InterviewSlotID },
                created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            return StatusCode(500, "An error occurred while creating the interview slot.");
        }
    }

    // Authorized roles: Teacher
    /// <summary>
    /// Create multiple back-to-back interview slots inside a single time window.
    /// When <see cref="CreateBulkInterviewSlotsDto.Quantity"/> is greater than 1, the window
    /// is split evenly into N consecutive slots sharing the same place and interview type
    /// (e.g. 5 slots inside 1 hour produce five 12-minute slots).
    /// </summary>
    /// <param name="dto">The bulk creation payload.</param>
    [HttpPost("bulk")]
    public async Task<ActionResult<IReadOnlyList<InterviewSlots>>> CreateBulk(
        [FromBody] CreateBulkInterviewSlotsDto dto)
    {
        if (dto is null)
        {
            return BadRequest("Request body is required.");
        }

        if (dto.Quantity <= 0)
        {
            return BadRequest("Quantity must be at least 1.");
        }

        if (dto.TimeEnd <= dto.TimeStart)
        {
            return BadRequest("TimeEnd must be later than TimeStart.");
        }

        try
        {
            var isValidRange = await _service.ValidateSlotTimesAsync(
                dto.JobId,
                dto.TimeStart,
                dto.TimeEnd);

            if (!isValidRange)
            {
                return BadRequest("The specified time range is invalid or overlaps existing slots.");
            }

            var created = await _service.CreateBulkSlotsAsync(dto);
            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            return StatusCode(500, "An error occurred while creating the interview slots.");
        }
    }

    // Authorized roles: Teacher
    /// <summary>
    /// Update an existing interview slot.
    /// </summary>
    /// <param name="id">The interview slot identifier.</param>
    /// <param name="dto">The updated interview slot data.</param>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<InterviewSlots>> Update(
        [FromRoute] long id,
        [FromBody] UpdateInterviewSlotDto dto)
    {
        if (dto is null)
        {
            return BadRequest("Request body is required.");
        }

        if (dto.Id != 0 && dto.Id != id)
        {
            return BadRequest("Id in body must match route.");
        }

        if (dto.TimeEnd <= dto.TimeStart)
        {
            return BadRequest("TimeEnd must be later than TimeStart.");
        }

        dto.Id = id;

        try
        {
            var updated = await _service.UpdateSlotDetailsAsync(dto);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            return StatusCode(500, "An error occurred while updating the interview slot.");
        }
    }

    // Authorized roles: Teacher
    /// <summary>
    /// Delete an interview slot by its identifier.
    /// </summary>
    /// <param name="id">The interview slot identifier.</param>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        try
        {
            var deleted = await _service.DeleteSlotAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "An error occurred while deleting the interview slot.");
        }
    }
}
