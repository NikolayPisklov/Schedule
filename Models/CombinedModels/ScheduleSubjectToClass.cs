namespace Schedule.Models.CombinedModels
{
    public class ScheduleSubjectToClass
    {
        public int Id { get; set; }
        public int FkTs { get; set; }
        public int FkSchedule { get; set; }
        public int FkTeacher { get; set; }
        public double Hours { get; set; }
        public double DifficultCoefficient { get; set; }
    }
}
