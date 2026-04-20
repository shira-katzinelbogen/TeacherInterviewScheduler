namespace SchedulingService.Enums;

/// <summary>
/// Reason for availability/unavailability (Personal / Interview and more) 
/// - shown to the manager, private to the student.
/// StudentAvailability.reason_status
/// </summary>
public enum AvailabilityReasonKind
{
    /// <summary>פרטי (e.g. תור לרופא)</summary>
    Personal = 0,

    /// <summary>ראיון (e.g. ראיון שקבעה מישהי)</summary>
    Interview = 1
}

/// <summary>
/// Student availability status: Available / Unavailable.
/// StudentAvailability.status
/// </summary>
public enum AvailabilityStatus
{
    /// <summary>זמין</summary>
    Available = 0,

    /// <summary>לא זמין</summary>
    Unavailable = 1
}
