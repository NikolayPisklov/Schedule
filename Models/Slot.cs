namespace Schedule.Models
{
    public class Slot
    {
        public int Id { get; set; }
        public int FkSubjectToClass { get; set; }
        public int FkDay { get; set; }
        public int FkTime { get; set; }
        public int FkClassroom { get; set; }
    }
}
