using System;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.DTOs;

public class CreateInterviewSlotDto
{
    public long JobId { get; set; }
    public DateTime TimeStart { get; set; }
    public DateTime TimeEnd { get; set; }
    public string Place { get; set; } = string.Empty;
    public InterviewType InterviewType { get; set; }
}


