namespace SchedulingService.Models;
using SchedulingService.Enums;

public class StudentAvailability
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    /// <summary>Available / Not available (זמין / לא זמין)</summary>
    public AvailabilityStatus Status { get; set; } = 0;

    /// <summary>Student-provided reason (e.g. Doctor's appointment, Interview)</summary>
    public string ReasonStudent { get; set; } = string.Empty;

    /// <summary>Private/Interview - ENUM provided by another company</summary>
    public AvailabilityReasonKind ReasonStatus { get; set; } = 0;
}
