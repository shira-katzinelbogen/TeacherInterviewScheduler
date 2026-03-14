namespace SchedulingService.Enums;

/// <summary>
/// Interview status: Scheduled / Pending Approval / Completed / Cancelled.
/// ScheduledInterviews.InterviewStatus
/// </summary>
public enum InterviewStatus
{
    /// <summary>נקבע</summary>
    Scheduled = 0,

    /// <summary>ממתין לאישור</summary>
    PendingApproval = 1,

    /// <summary>בוצע</summary>
    Completed = 2,

    /// <summary>בוטל</summary>
    Cancelled = 3
}
