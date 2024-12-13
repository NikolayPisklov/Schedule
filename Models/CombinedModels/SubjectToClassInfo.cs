namespace Schedule.Models.CombinedModels
{
    public class SubjectToClassInfo
    {
        public int ScId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public double Hours { get; set; }
    }
}
