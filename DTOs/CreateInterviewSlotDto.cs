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

public static class CreateInterviewSlotMappers
{
    public static InterviewSlots ToEntity(
        this CreateInterviewSlotDto dto,
        long interviewSlotId = 0,
        SlotStatus slotStatus = SlotStatus.Unassigned)
    {
        return new InterviewSlots(
            interviewSlotID: interviewSlotId,
            jobID: dto.JobId,
            timeStart: dto.TimeStart,
            timeEnd: dto.TimeEnd,
            place: dto.Place,
            interviewType: dto.InterviewType,
            slotStatus: slotStatus);
    }
}

