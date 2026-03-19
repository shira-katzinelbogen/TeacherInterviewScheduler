using AutoMapper;
using SchedulingService.DTOs.ScheduledInterviews;
using SchedulingService.Models;

namespace SchedulingService.Mapping;

public sealed class ScheduledInterviewProfile : Profile
{
    public ScheduledInterviewProfile()
    {
        CreateMap<ScheduledInterview, ScheduledInterviewDto>()
            .ForMember(d => d.SlotId, opt => opt.MapFrom(s => s.InterviewSlotID))
            .ForMember(d => d.InterviewStatus, opt => opt.MapFrom(s => s.InterviewStatus.ToString()))
            .ForMember(d => d.SlotStart, opt => opt.MapFrom(s => s.InterviewSlot.TimeStart))
            .ForMember(d => d.SlotEnd, opt => opt.MapFrom(s => s.InterviewSlot.TimeEnd))
            .ForMember(d => d.Place, opt => opt.MapFrom(s => s.InterviewSlot.Place))
            .ForMember(d => d.InterviewType, opt => opt.MapFrom(s => s.InterviewSlot.InterviewType.ToString()));
    }
}

