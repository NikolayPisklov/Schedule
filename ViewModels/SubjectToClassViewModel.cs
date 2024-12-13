using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Models.CombinedModels;
using System.Collections.ObjectModel;

namespace Schedule.ViewModels
{
    public class SubjectToClassViewModel : ViewModelBase
    {
        public ObservableCollection<TeacherSubjectInfo> SubjectsInfo { get; set; } = new ObservableCollection<TeacherSubjectInfo>();
        public ObservableCollection<SubjectToClassInfo> AssignedSubjects { get; set; } = new ObservableCollection<SubjectToClassInfo>();
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
        public SubjectToClassInfo? SelectedSubjectToClass 
        {
            get 
            {
                return _selectedSubjectToClass;
            }
            set 
            {
                _selectedSubjectToClass = value;
                RaisePropertyChange();
            }
        }
        public ScheduleJoin ScheduleInfo { get; set; }
        public DelegateCommand InsertCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        private readonly ITeacherSubjectDataProvider _tsDataProvider;
        private readonly ISubjectToClassDataProvider _subjectToClassDataProvider;
        private TeacherSubjectInfo? _selectedTs;
        private double? _hours;
        private SubjectToClassInfo? _selectedSubjectToClass;

        public SubjectToClassViewModel(ScheduleJoin info, ITeacherSubjectDataProvider tsProvider, ISubjectToClassDataProvider scProvider) 
        {
            ScheduleInfo = info;
            _tsDataProvider = tsProvider;
            _subjectToClassDataProvider = scProvider;
            InsertCommand = new DelegateCommand(InsertSubjectToClass);
            DeleteCommand = new DelegateCommand(DeleteSubjectToClass);
        }

        public async override Task LoadAsync() 
        {
            var subjectsInfo = await _tsDataProvider.GetAllTeachersSubjectsAsync();
            var assignedSub = await _subjectToClassDataProvider.GetAllForClassAsync(ScheduleInfo.ClassId);
            if (subjectsInfo != null) 
            {
                foreach (var subjectInfo in subjectsInfo) 
                {
                    SubjectsInfo.Add(subjectInfo);
                }
            }
            if(assignedSub != null) 
            {
                foreach (var subjectToClassInfo in assignedSub) 
                {
                    AssignedSubjects.Add(subjectToClassInfo);
                }
            }
        }
        public async void InsertSubjectToClass(object? obj) 
        {
            //add validation and success window
            if (SelectedTs is not null && Hours is not null) 
            {
                await _subjectToClassDataProvider.InsertAsync(ScheduleInfo.ScheduleId, SelectedTs.Id, (double)Hours);
                var newSubjectToClass = await _subjectToClassDataProvider.GetLatestSubjectToClassAsync(ScheduleInfo.ClassId);
                AssignedSubjects.Add(newSubjectToClass);
            }
            
        }
        public async void DeleteSubjectToClass(object? obj)
        {
            // add window about success
            if(SelectedSubjectToClass is not null) 
            {
                await _subjectToClassDataProvider.DeleteSubjectToClassAsync(SelectedSubjectToClass.ScId);
                AssignedSubjects.Remove(SelectedSubjectToClass);
            }
        }
    }
}
