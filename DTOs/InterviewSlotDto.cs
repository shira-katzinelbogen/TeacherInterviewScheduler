using System;
using SchedulingService.Enums;

namespace SchedulingService.DTOs;

public sealed class InterviewSlotDto
{
    public long JobId { get; set; }
    public DateTime TimeStart { get; set; }
    public DateTime TimeEnd { get; set; }
    public string Place { get; set; } = string.Empty;
    public InterviewType InterviewType { get; set; }
    public SlotStatus SlotStatus { get; set; }
}

