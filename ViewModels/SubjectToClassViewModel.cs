using Schedule.DataProviders;
using Schedule.Models.CombinedModels;
using System.Collections.ObjectModel;

namespace Schedule.ViewModels
{
    public class SubjectToClassViewModel : ViewModelBase
    {
        public ObservableCollection<TeacherSubjectInfo> SubjectsInfo = new ObservableCollection<TeacherSubjectInfo>(); 
        public ScheduleJoin ScheduleInfo { get; set; }

        private readonly ITeacherSubjectDataProvider _tsDataProvider;
        public SubjectToClassViewModel(ScheduleJoin info, ITeacherSubjectDataProvider tsProvider) 
        {
            ScheduleInfo = info;
            _tsDataProvider = tsProvider;
        }

        public async override Task LoadAsync() 
        {
            var subjectsInfo = await _tsDataProvider.GetAllTeachersSubjectsAsync();
            if (subjectsInfo != null) 
            {
                foreach (var subjectInfo in subjectsInfo) 
                {
                    SubjectsInfo.Add(subjectInfo);
                }
            }
        }
    }
}
