using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchedulingService.DTOs.InterviewSlots;
using SchedulingService.BLL.Repositories;
using SchedulingService.Enums;
using SchedulingService.Models;

namespace SchedulingService.BLL.Mapping;
{
    public class InterviewSlotsMapper : Profile
    {
        public InterviewSlotsMapper()
        {
            CreateMap<InterviewSlot, CreateInterviewSlotDto>();
            CreateMap<CreateInterviewSlotDto, InterviewSlot>();
            
            CreateMap<InterviewSlot, UpdateInterviewSlotDto>();
            CreateMap<UpdateInterviewSlotDto, InterviewSlot>();
        }
    }
}