using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
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
        public List<SlotInfo> SlotsForTheView { get; set; } = new List<SlotInfo>();

        public int ClassesCount { get; set; }

        private readonly IScheduleDataProvider _dataProvider;
        private readonly ISubjectToClassDataProvider _subjectToClassDataProvider;
        private readonly ISlotDataProvider _slotDataProvider;

        public DelegateCommand OpenWindowForAssigningSubjectsCommand { get; }
        public DelegateCommand CreateScheduleCommand { get; }

        public ScheduleViewModel(IScheduleDataProvider scheduleDataProvider, ISubjectToClassDataProvider scdp, ISlotDataProvider slotDataProvider) 
        {
            _dataProvider = scheduleDataProvider;
            _subjectToClassDataProvider = scdp;
            _slotDataProvider = slotDataProvider;
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
        public async void CreateSchedule(object? obj) //For now it is for one class
        {
            YearSubjectsToClass = await _subjectToClassDataProvider.GetAllSubjectToClassForAYear(2024);
            
            var list = YearSubjectsToClass.OrderByDescending(x => x.DifficultCoefficient).OrderBy(x => x.FkSchedule).ToList();
            var difficultSubjects = list.Where(x => x.DifficultCoefficient >= 1.7).ToList();
            var otherSubjects = list.Where(x => x.DifficultCoefficient < 1.7).ToList();
            
            double sumOfHours = list.Sum(x => x.Hours);
            double avgHours = Math.Round(sumOfHours / 5);
            int i = 0;
            //at the beggining diff subjects and if there is no place add them to easy. Then place easy subjects into schedule
            while (i < list.Count) 
            {
                var slotInfo = list[i];
                var availableSlots = SlotsAvailability.Where(x => x.Value == true);
                foreach(var slot in availableSlots) 
                {
                    if(IsSlotEmpty(slotInfo, slot.Key) && slot.Value == true && slot.Key.time <= avgHours) 
                    {
                        //Adding to list of global taken slots. Get slot to taken in the dictionary, - hour
                        SlotsAvailability[slot.Key] = false;
                        AddSlotToTakenSlots(slotInfo, slot.Key);
                        list[i].Hours -= 1;
                        break;
                    }
                }
                if (list[i].Hours < 1) 
                {
                    i++;
                }
            }
            OnSchedulingCompleted();
        }
        public async void OnSchedulingCompleted() 
        {
            foreach (var slot in TakenSlots) 
            {
                await _slotDataProvider.InsertSlotAsync(slot);
            }
            var slotInfos = await _slotDataProvider.GetSlotsInfoForScheduleAsync(2024);
            foreach(var slot in slotInfos) 
            {
                SlotsForTheView.Add(slot);
            }
            Messanger.Instance.ScheduleDoneSend();
        }
        private bool IsSlotEmpty(ScheduleSubjectToClass slotInfo, (int, int) dayTime)
        {
            var takenSlot = TakenSlots.FirstOrDefault(x => x.TeacherId == slotInfo.FkTeacher && x.ScheduleId == slotInfo.FkSchedule
                && x.DayId == dayTime.Item1 && x.TimeId == dayTime.Item2);
            if (takenSlot == null) 
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        private void AddSlotToTakenSlots(ScheduleSubjectToClass slotInfo, (int, int) dayTime) 
        {
            TakenSlotInfo newSlot = new TakenSlotInfo() 
            {
                SubjectToClassId = slotInfo.Id,
                ScheduleId = slotInfo.FkSchedule,
                TeacherSubjectId = slotInfo.FkTs,
                TeacherId = slotInfo.FkTeacher,
                ClassroomId = 1, // later add for classroom constraint
                DayId = dayTime.Item1,
                TimeId = dayTime.Item2
            };
            TakenSlots.Add(newSlot);
        }

    }
}
