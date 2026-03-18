using AutoMapper;
using SchedulingService.DTOs.StudentAvailability;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.Mapping;

public sealed class StudentAvailabilityProfile : Profile
{
    public StudentAvailabilityProfile()
    {
        CreateMap<CreateStudentAvailabilityDto, StudentAvailability>()
            .ConstructUsing(s => new StudentAvailability(
                0,
                s.StudentId,
                s.StartTime,
                s.EndTime,
                s.Status,
                s.ReasonStudent ?? string.Empty,
                AvailabilityReasonKind.Personal));

        CreateMap<UpdateStudentAvailabilityDto, StudentAvailability>()
            .ForMember(d => d.StudentId, opt => opt.Ignore())
            .ForMember(d => d.ReasonStatus, opt => opt.Ignore())
            .ForMember(
                d => d.ReasonStudent,
                opt =>
                {
                    opt.PreCondition(s => s.ReasonStudent is not null);
                    opt.MapFrom(s => s.ReasonStudent!);
                });
    }
}

