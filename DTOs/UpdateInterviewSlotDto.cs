using System;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.DTOs;

public class UpdateInterviewSlotDto
{
    public long Id { get; set; }
    public long JobId { get; set; }
    public DateTime TimeStart { get; set; }
    public DateTime TimeEnd { get; set; }
    public string Place { get; set; } = string.Empty;
    public InterviewType InterviewType { get; set; }
    public SlotStatus SlotStatus { get; set; }
}

public static class UpdateInterviewSlotMappers
{
    public static UpdateInterviewSlotDto FromEntity(InterviewSlots entity)
    {
        return new UpdateInterviewSlotDto
        {
            Id = entity.InterviewSlotID,
            JobId = entity.JobID,
            TimeStart = entity.TimeStart,
            TimeEnd = entity.TimeEnd,
            Place = entity.Place,
            InterviewType = entity.InterviewType,
            SlotStatus = entity.SlotStatus
        };
    }

    public static void ApplyToEntity(this UpdateInterviewSlotDto dto, InterviewSlots entity)
    {
        entity.JobID = dto.JobId;
        entity.TimeStart = dto.TimeStart;
        entity.TimeEnd = dto.TimeEnd;
        entity.Place = dto.Place;
        entity.InterviewType = dto.InterviewType;
        entity.SlotStatus = dto.SlotStatus;
    }
}

