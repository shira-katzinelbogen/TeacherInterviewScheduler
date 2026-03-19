using SchedulingService.Enums;

namespace SchedulingService.DTOs.StudentAvailability;

public sealed class CreateStudentAvailabilityDto
{
    public long StudentId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AvailabilityStatus Status { get; set; }
    public string ReasonStudent { get; set; } = string.Empty;
}

