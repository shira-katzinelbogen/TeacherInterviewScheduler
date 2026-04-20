using AutoMapper;
using SchedulingService.DTOs.InterviewSlots;
using SchedulingService.Models;

namespace SchedulingService.BLL.Mapping;

public sealed class InterviewSlotsDtoProfile : Profile
{
    public InterviewSlotsDtoProfile()
    {
        CreateMap<InterviewSlots, InterviewSlotsDTO>()
            .ForMember(d => d.InterviewSlotId, opt => opt.MapFrom(s => s.InterviewSlotID))
            .ForMember(d => d.JobId, opt => opt.MapFrom(s => s.JobID));

        CreateMap<InterviewSlotsDTO, InterviewSlots>()
            .ConstructUsing(s => new InterviewSlots(
                s.InterviewSlotId,
                s.JobId,
                s.TimeStart,
                s.TimeEnd,
                s.Place ?? string.Empty,
                s.InterviewType,
                s.SlotStatus));
    }
}

