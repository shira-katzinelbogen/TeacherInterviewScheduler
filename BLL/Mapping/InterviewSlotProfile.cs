using AutoMapper;
using SchedulingService.DTOs;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.Mapping;

public sealed class InterviewSlotProfile : Profile
{
    public InterviewSlotProfile()
    {
        CreateMap<CreateInterviewSlotDto, InterviewSlots>()
            .ConstructUsing(s => new InterviewSlots(
                0,
                s.JobId,
                s.TimeStart,
                s.TimeEnd,
                s.Place ?? string.Empty,
                s.InterviewType,
                SlotStatus.Unassigned));

        CreateMap<UpdateInterviewSlotDto, InterviewSlots>()
            .ForMember(d => d.InterviewSlotID, opt => opt.Ignore())
            .ForMember(
                d => d.Place,
                opt =>
                {
                    opt.PreCondition(s => !string.IsNullOrWhiteSpace(s.Place));
                    opt.MapFrom(s => s.Place!);
                });
    }
}

