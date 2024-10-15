namespace Schedule.Models
{
    public class TeacherSubject
    {
        public int Id { get; set; }
        public int FkTeacher { get; set; }
        public int FkSubject { get; set; }

    }
}
