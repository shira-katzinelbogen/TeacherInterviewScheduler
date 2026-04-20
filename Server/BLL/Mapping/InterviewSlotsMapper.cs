using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SchedulingService.DTOs.InterviewSlots;
using SchedulingService.BLL.Repositories;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.BLL.Mapping
{
    public class InterviewSlotsMapper : Profile
    {
        public InterviewSlotsMapper()
        {
            CreateMap<InterviewSlots, CreateInterviewSlotDto>();
            CreateMap<CreateInterviewSlotDto, InterviewSlots>();
            
            CreateMap<InterviewSlots, UpdateInterviewSlotDto>();
            CreateMap<UpdateInterviewSlotDto, InterviewSlots>();
        }
    }
}