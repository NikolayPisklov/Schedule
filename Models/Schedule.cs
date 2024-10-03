namespace Schedule.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int FkClass { get; set; }
        public int Year { get; set; }
        public int Semester { get; set; }
    }
}
