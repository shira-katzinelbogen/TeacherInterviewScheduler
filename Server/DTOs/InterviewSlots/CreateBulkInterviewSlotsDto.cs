using System;
using SchedulingService.Enums;

namespace SchedulingService.DTOs.InterviewSlots;

/// <summary>
/// Payload for creating multiple consecutive interview slots in a single time window.
/// The window [TimeStart, TimeEnd] is split evenly into <see cref="Quantity"/> back-to-back
/// slots that share the same place and interview type.
/// </summary>
public class CreateBulkInterviewSlotsDto
{
    public long JobId { get; set; }
    public DateTime TimeStart { get; set; }
    public DateTime TimeEnd { get; set; }
    public string Place { get; set; } = string.Empty;
    public InterviewType InterviewType { get; set; }

    /// <summary>How many slots to generate inside the time window. Must be at least 1.</summary>
    public int Quantity { get; set; } = 1;
}
