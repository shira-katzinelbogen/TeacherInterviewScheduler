using SchedulingService.Enums;

namespace SchedulingService.DTOs.StudentAvailability;

public sealed class BulkUpdateStudentAvailabilityDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public AvailabilityStatus Status { get; set; }
    public string? Reason { get; set; }
}

