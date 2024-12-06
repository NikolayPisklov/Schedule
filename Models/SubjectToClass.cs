namespace Schedule.Models
{
    public class SubjectToClass
    {
        public int Id { get; set; }
        public int FkTs { get; set; }
        public int FkSchedule { get; set; }
        public double Hours { get; set; }
    }
}
