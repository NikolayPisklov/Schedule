namespace Schedule.Models
{
    public class Slot
    {
        public int Id { get; set; }
        public int FkSchedule { get; set; }
        public int FkTeacher { get; set; }
        public int FkSubject { get; set; }
        public int FkTime{ get; set; }
        public int FkClassroom{ get; set; }
        public DateOnly Date{ get; set; }
    }
}
