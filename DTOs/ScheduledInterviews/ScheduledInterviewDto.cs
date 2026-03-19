namespace SchedulingService.DTOs.ScheduledInterviews;

/// <summary>
/// Output DTO representing a scheduled interview and its associated slot details.
/// </summary>
public sealed class ScheduledInterviewDto
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public long SlotId { get; set; }

    public string InterviewStatus { get; set; } = string.Empty;
    public string? Comments { get; set; }

    public DateTime SlotStart { get; set; }
    public DateTime SlotEnd { get; set; }
    public string? Place { get; set; }
    public string? InterviewType { get; set; }
}

