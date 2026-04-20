using System;
using SchedulingService.Enums;

namespace SchedulingService.DTOs.InterviewSlots;

public sealed class InterviewSlotsDTO
{
    public long InterviewSlotId { get; set; }
    public long JobId { get; set; }
    public DateTime TimeStart { get; set; }
    public DateTime TimeEnd { get; set; }
    public string Place { get; set; } = string.Empty;
    public InterviewType InterviewType { get; set; }
    public SlotStatus SlotStatus { get; set; }
}

