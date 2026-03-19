namespace SchedulingService.DTOs.ScheduledInterviews;

/// <summary>
/// Input DTO for scheduling an interview for a student into a specific slot.
/// </summary>
public sealed class ScheduleInterviewDto
{
    public long StudentId { get; set; }
    public long SlotId { get; set; }
    public string? Comments { get; set; }
}

