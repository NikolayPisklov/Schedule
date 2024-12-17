using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Schedule.Classes;
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
        public List<Teacher> Teachers { get; set; } = new List<Teacher>();
        public ObservableCollection<Class> Classes { get; set; } = new ObservableCollection<Class>();

        //Collections for schedule
        public List<ScheduleSubjectToClass> LessonsForClass { get; set; } = new List<ScheduleSubjectToClass>();
        public List<int> DaysIds { get; set; } = new List<int>();
        public List<int> TimeIds { get; set; } = new List<int>();
        public Dictionary<(int day, int time, int clas), bool> ClassSlotsAvailability { get; set; } 
            = new Dictionary<(int day, int time, int clas), bool>();
        public Dictionary<(int day, int time, int teacher), bool> TeacherSlotsAvailability { get; set; }
            = new Dictionary<(int day, int time, int teacher), bool>();
        public List<TakenSlotInfo> TakenSlots { get; set; } = new List<TakenSlotInfo>();
        public List<ScheduleSubjectToClass> YearSubjectsToClass = new List<ScheduleSubjectToClass>();
        public List<SlotInfo> SlotsForTheView { get; set; } = new List<SlotInfo>();
        public List<AvgHoursForClass> AvgHoursForClass { get; set; } = new List<AvgHoursForClass>();

        public int ClassesCount { get; set; }

        private readonly IScheduleDataProvider _dataProvider;
        private readonly ISubjectToClassDataProvider _subjectToClassDataProvider;
        private readonly ISlotDataProvider _slotDataProvider;
        private readonly ITeacherDataProvider _teacherDataProvider;

        public DelegateCommand OpenWindowForAssigningSubjectsCommand { get; }
        public DelegateCommand CreateScheduleCommand { get; }

        public ScheduleViewModel(IScheduleDataProvider scheduleDataProvider, ISubjectToClassDataProvider scdp, ISlotDataProvider slotDataProvider,
            ITeacherDataProvider tdp) 
        {
            _dataProvider = scheduleDataProvider;
            _subjectToClassDataProvider = scdp;
            _slotDataProvider = slotDataProvider;
            _teacherDataProvider = tdp;
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
            var teachers = await _teacherDataProvider.GetAllTeachersAsync();
            if(teachers is not null) 
            {
                foreach (var teacher in teachers) 
                {
                    Teachers.Add(teacher);
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
            if(DaysIds.Any() && TimeIds.Any() && SchedulesInfo.Any()) 
            {
                foreach (var schedule in SchedulesInfo) 
                {
                    foreach (var day in DaysIds)
                    {
                        foreach (var time in TimeIds)
                        {
                            ClassSlotsAvailability.Add((day, time, schedule.ScheduleId), true);
                        }
                    }
                }
                foreach (var teacher in Teachers)
                {
                    foreach (var day in DaysIds)
                    {
                        foreach (var time in TimeIds)
                        {
                            TeacherSlotsAvailability.Add((day, time, teacher.Id), true);
                        }
                    }
                }

            }
        }
        public async void CreateSchedule(object? obj) //For now it is for one class
        {
            YearSubjectsToClass = await _subjectToClassDataProvider.GetAllSubjectToClassForAYear(2024);
            
            var list = YearSubjectsToClass.ToList();
            CalculateAvgHoursForClasses(list);
            var lessons = CreateLessonsForClass(list);
            
            var difficulLessons = lessons.Where(x => x.DifficultCoefficient >= 1.7).ToList();
            var otherLessons = lessons.Where(x => x.DifficultCoefficient < 1.7).ToList();

            var random = new Random();
            difficulLessons = difficulLessons.OrderBy(x=>random.Next()).ToList();
            
            int i = 0;
            //for one class only
            while (i < difficulLessons.Count) 
            {
                var slotInfo = difficulLessons[i];
                var availableSlots = ClassSlotsAvailability.Where(x => x.Value == true && x.Key.clas == slotInfo.FkSchedule
                    && x.Key.day >= 2 && x.Key.day <= 4).ToList();//add conddition for class id key item
                var avgHours = AvgHoursForClass.First(x => x.FkSchedule == slotInfo.FkSchedule);
                foreach(var slot in availableSlots) 
                {
                    if (!IsThereSlotInPriorityDays(availableSlots))
                    {
                        otherLessons.Add(slotInfo);
                        i++;
                        break;
                    }
                    if (IsTeacherFree(slotInfo, slot.Key) && IsDayHoursNormal(avgHours.AvgHours, slot.Key)) 
                    {
                        ClassSlotsAvailability[slot.Key] = false;
                        AddSlotToTakenSlots(slotInfo, (slot.Key.day, slot.Key.time));
                        i++;
                        break;
                    }      
                }
            }
            i = 0;
            otherLessons = otherLessons.OrderBy(x => random.Next()).ToList();
            while (i < otherLessons.Count) 
            {
                var slotInfo = otherLessons[i];
                var availableSlots = ClassSlotsAvailability.Where(x => x.Value == true 
                    && x.Key.clas == slotInfo.FkSchedule).ToList();//add conddition for class id key item
                var avgHours = AvgHoursForClass.First(x => x.FkSchedule == slotInfo.FkSchedule);
                foreach (var slot in availableSlots) 
                {
                    if(IsTeacherFree(slotInfo, slot.Key) && IsDayHoursNormal(avgHours.AvgHours, slot.Key)) 
                    {
                        ClassSlotsAvailability[slot.Key] = false;
                        AddSlotToTakenSlots(slotInfo, (slot.Key.day, slot.Key.time));
                        i++;
                        break;
                    }
                }
            }
            OnSchedulingCompleted();
        }
        private void CalculateAvgHoursForClasses(List<ScheduleSubjectToClass> subjectsToClass) 
        {
            foreach (var schedule in SchedulesInfo) 
            {
                double sumOfHours = subjectsToClass.Where(x => x.FkSchedule == schedule.ScheduleId).Sum(x => x.Hours);
                double hoursForClass;
                if (sumOfHours <= 5) 
                {
                    hoursForClass = sumOfHours;
                }
                else 
                {
                    hoursForClass = Math.Round(sumOfHours / 5);
                }   
                AvgHoursForClass avg = new AvgHoursForClass
                {
                    FkSchedule = schedule.ScheduleId,
                    AvgHours = hoursForClass
                };
                AvgHoursForClass.Add(avg);
            }   
        }
        private bool IsDayHoursNormal(double avgHours, (int, int, int) clasKey)
        {
            var classTakenSkots = TakenSlots.Where(x=>x.DayId == clasKey.Item1 
                && x.ScheduleId == clasKey.Item3).ToList();
            var countHoursInDay = classTakenSkots.Count();
            if (countHoursInDay < avgHours)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        private bool IsTeacherFree(ScheduleSubjectToClass lesson, (int, int, int) clasKey)
        {
            var teacherKey = (clasKey.Item1, clasKey.Item2, lesson.FkTeacher);
            bool isteacherFree = TeacherSlotsAvailability.TryGetValue(teacherKey, out bool value);
            return isteacherFree;
        }
        private bool IsThereSlotInPriorityDays(List<KeyValuePair<(int, int, int), bool>> availableSlots) 
        {
            if (availableSlots.Any(x => x.Key.Item1 >= 2 && x.Key.Item1 <= 4)) 
            {
                return true;
            }
            else 
            { 
                return false; 
            }
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
        private List<ScheduleSubjectToClass> CreateLessonsForClass(List<ScheduleSubjectToClass> list) 
        {
            var lessons = new List<ScheduleSubjectToClass>();
            foreach (var subject in list) 
            {
                while (subject.Hours >= 1) //change for 0.5 lessons as well
                {
                    lessons.Add(subject);
                    subject.Hours -= 1;
                }
            }
            return lessons;
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
