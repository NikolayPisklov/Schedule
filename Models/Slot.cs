namespace Schedule.Models
{
    public class Slot
    {
        public int Id { get; set; }
        public int FkSchedule { get; set; }
        public int FkTeacherSubject { get; set; }
        public int FkTime{ get; set; }
        public int FkClassroom{ get; set; }
        public DateOnly Date{ get; set; }
    }
}
