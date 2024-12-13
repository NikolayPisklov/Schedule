using System.Collections.ObjectModel;
using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Models;
using Schedule.Models.CombinedModels;
using Schedule.Models.UI;
using Schedule.Views;

namespace Schedule.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {

        public ObservableCollection<ScheduleJoin> SchedulesInfo { get; set; } = new ObservableCollection<ScheduleJoin>();
        public ObservableCollection<Class> Classes { get; set; } = new ObservableCollection<Class>();

        //Collections for schedule
        public List<int> DaysIds { get; set; } = new List<int>();
        public List<int> TimeIds { get; set; } = new List<int>();
        public Dictionary<(int day, int time), bool> SlotsAvailability { get; set; } = new Dictionary<(int day, int time), bool>();
        public List<TakenSlotInfo> TakenSlots { get; set; } = new List<TakenSlotInfo>();
        public List<ScheduleSubjectToClass> YearSubjectsToClass = new List<ScheduleSubjectToClass>();

        public int ClassesCount { get; set; }

        private readonly IScheduleDataProvider _dataProvider;
        private readonly ISubjectToClassDataProvider _subjectToClassDataProvider;

        public DelegateCommand OpenWindowForAssigningSubjectsCommand { get; }
        public DelegateCommand CreateScheduleCommand { get; }

        public ScheduleViewModel(IScheduleDataProvider scheduleDataProvider, ISubjectToClassDataProvider scdp) 
        {
            _dataProvider = scheduleDataProvider;
            _subjectToClassDataProvider = scdp;
            OpenWindowForAssigningSubjectsCommand = new DelegateCommand(OpenWindowForAssigningSubjects);
            CreateScheduleCommand = new DelegateCommand(CreateSchedule);
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
            ClassesCount = SchedulesInfo.Count;
            SetDaysIds();
            SetTimeIds();
            SetEmptySlots();
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
        private async void SetDaysIds() 
        {
            DaysIds = await _dataProvider.GetDaysId();
        }
        private async void SetTimeIds() 
        {
            TimeIds = await _dataProvider.GetTimeId();
        }
        private void SetEmptySlots() 
        {
            if(DaysIds.Any() && TimeIds.Any()) 
            {
                foreach (var day in DaysIds) 
                {
                    foreach(var time in TimeIds) 
                    {
                        SlotsAvailability.Add((day, time), true);
                    }
                }
            }
        }
        public async void CreateSchedule(object? obj) //For now it is for one class ()
        {
            YearSubjectsToClass = await _subjectToClassDataProvider.GetAllSubjectToClassForAYear(2024);
            var list = YearSubjectsToClass.OrderByDescending(x => x.DifficultCoefficient).OrderBy(x => x.FkSchedule).ToList();
            //Algorithm !!!THIS IS ONLY FOR ONE CLASS!!!
            int i = 0;
            while (i < list.Count) 
            {
                var subject = list[i];
                bool isAppointed = false;
                var availableSlots = SlotsAvailability.Where(x => x.Value);
                foreach(var slot in availableSlots) 
                {
                    if(IsTeacherAvailable() && IsClassAvailable()) 
                    {
                        //Adding to list of global taken slots. Get slot to taken in the dictionary
                        isAppointed = true;
                    }
                }
                if(isAppointed) 
                {
                    i++;
                }
            }
        }
        private bool IsTeacherAvailable() //searching through global slot list
        {
            return true;
        }
        private bool IsClassAvailable() //searching through global slot list
        {
            return true;
        }
    }
}
