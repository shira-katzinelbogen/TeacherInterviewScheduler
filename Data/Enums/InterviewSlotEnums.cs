namespace SchedulingService.Enums;

/// <summary>
/// Was a student assigned to this slot?
/// InterviewSlots.slotStatus
/// </summary>
public enum SlotStatus
{
    /// <summary>פנוי</summary>
    Unassigned = 0,

    /// <summary>תפוס</summary>
    Assigned = 1
}

/// <summary>
/// Interview type: Technical / Professional / Personal and more.
/// InterviewSlots.interview_type
/// </summary>
public enum InterviewType
{
    /// <summary>טכני</summary>
    Technical = 0,

    /// <summary>מקצועי</summary>
    Professional = 1,

    /// <summary>אישי</summary>
    Personal = 2,

    /// <summary>אחר</summary>
    Other = 3
}
