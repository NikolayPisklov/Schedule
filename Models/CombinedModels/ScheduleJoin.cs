namespace Schedule.Models.CombinedModels
{
    public class ScheduleJoin
    {
        public int ScheduleId { get; set; }
        public int ClassId { get; set; }
        public string ClassTitle { get; set; } = string.Empty;
        public int ScheduleYear { get; set; }
        public int ClassYear { get; set; }
    }
}
