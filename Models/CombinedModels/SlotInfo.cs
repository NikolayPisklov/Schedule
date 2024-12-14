namespace Schedule.Models.CombinedModels
{
    public class SlotInfo
    {
         public int ClassId { get; set; }
         public int DayId { get; set; }
         public int TimeId { get; set; }
         public string FullName { get; set; } = string.Empty;
         public string SubjectTitle { get; set; } = string.Empty;
    }
}
