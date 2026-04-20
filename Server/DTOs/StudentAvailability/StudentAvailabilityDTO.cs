using System;
using SchedulingService.Enums;

namespace SchedulingService.DTOs.StudentAvailability;

public sealed class StudentAvailabilityDto
{
    public long StudentAvailabilityId { get; set; }
    public long StudentId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AvailabilityStatus Status { get; set; }
    public string ReasonStudent { get; set; } = string.Empty;
    public AvailabilityReasonKind ReasonStatus { get; set; }
}

