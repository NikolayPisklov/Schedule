namespace Schedule.Classes
{
    public class AvgHoursForClass
    {
        public int FkSchedule { get; set; }
        public double AvgHours { get; set; }
        public double DivideRest { get; set; }
        public int MaxLessonADay { get; set; }
    }
}
