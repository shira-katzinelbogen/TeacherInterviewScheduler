namespace SchedulingService.Models;

using System.ComponentModel.DataAnnotations;
using SchedulingService.Enums;

public class StudentAvailability
{
    [Key]
    public long Id { get; set; }
    public long StudentId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    /// <summary>Available / Not available (זמין / לא זמין)</summary>
    public AvailabilityStatus Status { get; set; } = 0;

    /// <summary>Student-provided reason </summary>
    public string ReasonStudent { get; set; } = string.Empty;

    /// <summary>Private/Interview - ENUM </summary>
    public AvailabilityReasonKind ReasonStatus { get; set; } = 0;

    public StudentAvailability(long id, long studentId, DateTime startTime, DateTime endTime, AvailabilityStatus status, string reasonStudent, AvailabilityReasonKind reasonStatus)
    {
        Id = id;
        StudentId = studentId;
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
        ReasonStudent = reasonStudent;
        ReasonStatus = reasonStatus;
    }
}
