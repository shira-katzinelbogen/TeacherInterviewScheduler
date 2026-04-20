using System.ComponentModel.DataAnnotations;
using SchedulingService.Enums;

namespace SchedulingService.Models;

/// <summary>
/// Represents a scheduled interview that links a student to a specific interview slot.
/// </summary>
public class ScheduledInterview
{
    /// <summary>
    /// Primary key of the scheduled interview.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Identifier of the student assigned to this interview.
    /// </summary>
    public long StudentId { get; set; }

    /// <summary>
    /// Identifier of the interview slot this interview is scheduled into.
    /// </summary>
    public long InterviewSlotID { get; set; }

    /// <summary>
    /// Current status of the interview (e.g. Scheduled, Completed, Canceled).
    /// </summary>
    public InterviewStatus InterviewStatus { get; set; }

    /// <summary>
    /// Optional free‑text comments related to this interview.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Navigation property to the related interview slot.
    /// </summary>
    public InterviewSlots InterviewSlot { get; set; } = null!;
}
