using System.Collections.ObjectModel;
using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Models;
using Schedule.Models.CombinedModels;
using Schedule.Views;

namespace Schedule.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {
        public ObservableCollection<ScheduleJoin> SchedulesInfo {get; set;} = new ObservableCollection<ScheduleJoin>();//for adding a schedule to a class
        public ObservableCollection<Class> Classes { get; set; } = new ObservableCollection<Class>();// select classes with schedule for current year
        public List<DayOfTheWeek> DaysOfTheWeek { get; set;} = new List<DayOfTheWeek>();

        private readonly IScheduleDataProvider _dataProvider;

        public DelegateCommand OpenWindowForAssigningSubjectsCommand { get; }

        public ScheduleViewModel(IScheduleDataProvider scheduleDataProvider) 
        {
            _dataProvider = scheduleDataProvider;
            OpenWindowForAssigningSubjectsCommand = new DelegateCommand(OpenWindowForAssigningSubjects);
        }
        public async override Task LoadAsync()
        {
            var schedules = await _dataProvider.GetSchedulesForAYearAsync();
            if(schedules is not null) 
            {
                foreach(var schedule in schedules) 
                {
                    SchedulesInfo.Add(schedule);
                }
            }
        }
        public async void OpenWindowForAssigningSubjects(object? obj) 
        {
            var info = obj as ScheduleJoin;
            if (info is not null) 
            {
                SubjectToClassView newWindow = new SubjectToClassView();
                var viewModel = new SubjectToClassViewModel(info, new TeacherSubjectDataProvider(), new SubjectToClassDataProvider());
                newWindow.DataContext = viewModel;
                await viewModel.LoadAsync();
                newWindow.Show();
                
            }
        }
    }
}
