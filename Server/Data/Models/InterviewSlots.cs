namespace SchedulingService.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using SchedulingService.Enums;

    public class InterviewSlots
    {
        public InterviewSlots(long interviewSlotID, long jobID, DateTime timeStart, DateTime timeEnd, string place, InterviewType interviewType, SlotStatus slotStatus)
        {
            InterviewSlotID = interviewSlotID;
            JobID = jobID;
            TimeStart = timeStart;
            TimeEnd = timeEnd;
            Place = place;
            InterviewType = interviewType;
            SlotStatus = slotStatus;
        }

        [Key]
        public long InterviewSlotID { get; set; }
        public long JobID { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public string Place { get; set; } = string.Empty;
        public InterviewType InterviewType { get; set; }
        public SlotStatus SlotStatus { get; set; }
    }
}
