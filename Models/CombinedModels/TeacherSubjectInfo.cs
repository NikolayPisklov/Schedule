namespace Schedule.Models.CombinedModels
{
    public class TeacherSubjectInfo
    {
        public int Id { get; set; }
        public int FkTeacher { get; set; }
        public int FkSubject { get; set; }
        public string SubjectTitle { get; set; } = string.Empty;
        public string TeachersName { get; set; } = string.Empty;
    }
}
