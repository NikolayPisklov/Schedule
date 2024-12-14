namespace Schedule.Models.CombinedModels
{
    public class TakenSlotInfo
    {
        public int SubjectToClassId { get; set; }
        public int ScheduleId { get; set; }
        public int TeacherSubjectId { get; set; }
        public int TeacherId { get; set; }
        public int ClassroomId { get; set; }
        public int DayId { get; set; }
        public int TimeId { get; set; }
    }
}
