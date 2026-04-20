namespace SchedulingService.DTOs.ScheduledInterviews;

/// <summary>
/// Represents a student and the queried time range they are available for.
/// </summary>
public sealed class StudentWithAvailabilityDto
{
    public long StudentId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

