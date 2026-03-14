using SchedulingService.Enums;

namespace SchedulingService.Data.Entities;

/// <summary>
/// ראיון בפועל - הקישור בין ראיון לבת ששובצה אליו.
/// Links a student to an interview slot.
/// </summary>
public class ScheduledInterview
{
    public long Id { get; set; }

    /// <summary>
    /// קישור לתלמידה (טבלת Student / StudentAvailability - יוגדר כשהמודל יידחף).
    /// </summary>
    public long StudentId { get; set; }

    /// <summary>
    /// קישור לטבלת InterviewSlots - יוגדר כשהמודל יידחף.
    /// </summary>
    public long InterviewSlotId { get; set; }

    /// <summary>
    /// בוצע / בוטל / ממתין לאישור / נקבע.
    /// </summary>
    public InterviewStatus InterviewStatus { get; set; }

    /// <summary>
    /// הערות (אופציונלי).
    /// </summary>
    public string? Comments { get; set; }

    // Navigation properties - uncomment and fix type when Student and InterviewSlot entities exist:
    // public Student Student { get; set; } = null!;
    // public InterviewSlot InterviewSlot { get; set; } = null!;
}
