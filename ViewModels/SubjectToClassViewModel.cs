using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Models.CombinedModels;
using System.Collections.ObjectModel;

namespace Schedule.ViewModels
{
    public class SubjectToClassViewModel : ViewModelBase
    {
        public ObservableCollection<TeacherSubjectInfo> SubjectsInfo { get; set; } = new ObservableCollection<TeacherSubjectInfo>();
        public double? Hours 
        {
            get 
            {
                return _hours;
            }
            set 
            {
                _hours = value;
                RaisePropertyChange();
            } 
        }
        public TeacherSubjectInfo? SelectedTs
        {
            get 
            {
                return _selectedTs;
            }
            set 
            {
                _selectedTs = value;
                RaisePropertyChange();
            }
        }
        public ScheduleJoin ScheduleInfo { get; set; }
        public DelegateCommand InsertCommand { get; }
        private readonly ITeacherSubjectDataProvider _tsDataProvider;
        private readonly ISubjectToClassDataProvider _subjectToClassDataProvider;
        private TeacherSubjectInfo? _selectedTs;
        private double? _hours;

        public SubjectToClassViewModel(ScheduleJoin info, ITeacherSubjectDataProvider tsProvider, ISubjectToClassDataProvider scProvider) 
        {
            ScheduleInfo = info;
            _tsDataProvider = tsProvider;
            _subjectToClassDataProvider = scProvider;
            InsertCommand = new DelegateCommand(InsertSubjectToClass);
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
        public async void InsertSubjectToClass(object? obj) 
        {
            if(SelectedTs is not null && Hours is not null) 
            {
                await _subjectToClassDataProvider.InsertAsync(ScheduleInfo.ScheduleId, SelectedTs.Id, (double)Hours);
            }
            
        }
    }
}
