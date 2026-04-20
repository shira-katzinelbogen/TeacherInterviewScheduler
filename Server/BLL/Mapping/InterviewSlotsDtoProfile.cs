using AutoMapper;
using SchedulingService.DTOs.InterviewSlots;
using SchedulingService.Models;

namespace SchedulingService.BLL.Mapping;

public sealed class InterviewSlotsDtoProfile : Profile
{
    public InterviewSlotsDtoProfile()
    {
        CreateMap<InterviewSlots, IntervieSlotsDTO>()
            .ForMember(d => d.InterviewSlotId, opt => opt.MapFrom(s => s.InterviewSlotID))
            .ForMember(d => d.JobId, opt => opt.MapFrom(s => s.JobID));

        CreateMap<IntervieSlotsDTO, InterviewSlots>()
            .ConstructUsing(s => new InterviewSlots(
                interviewSlotID: s.InterviewSlotId,
                jobID: s.JobId,
                timeStart: s.TimeStart,
                timeEnd: s.TimeEnd,
                place: s.Place ?? string.Empty,
                interviewType: s.InterviewType,
                slotStatus: s.SlotStatus));
    }
}

