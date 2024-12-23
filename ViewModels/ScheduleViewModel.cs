using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
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
        private List<Teacher> Teachers { get; set; } = new List<Teacher>();

        //Collections for schedule
        private List<ScheduleSubjectToClass> LessonsForClass { get; set; } = new List<ScheduleSubjectToClass>();
        public List<int> DaysIds { get; set; } = new List<int>();
        public List<int> TimeIds { get; set; } = new List<int>();
        private Dictionary<(int day, int time, int clas), bool> ClassSlotsAvailability { get; set; } 
            = new Dictionary<(int day, int time, int clas), bool>();
        private Dictionary<(int day, int time, int teacher), bool> TeacherSlotsAvailability { get; set; }
            = new Dictionary<(int day, int time, int teacher), bool>();
        private List<TakenSlotInfo> TakenSlots { get; set; } = new List<TakenSlotInfo>();
        private List<ScheduleSubjectToClass> YearSubjectsToClass = new List<ScheduleSubjectToClass>();
        public List<SlotInfo> SlotsForTheView { get; set; } = new List<SlotInfo>();
        private List<AvgHoursForClass> AvgHoursForClass { get; set; } = new List<AvgHoursForClass>();

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
            var restOfTHeLessons = new List<ScheduleSubjectToClass>(); 

            var random = new Random();
            difficulLessons = difficulLessons.OrderBy(x=>random.Next()).ToList();
            //DIFFICULT LESSONS
            int i = 0;
            while (i < difficulLessons.Count) 
            {
                int startingI = i;
                var slotInfo = difficulLessons[i];
                var availableSlots = ClassSlotsAvailability.Where(x => x.Value == true && x.Key.clas == slotInfo.FkSchedule
                    && x.Key.day >= 2 && x.Key.day <= 4).ToList();
                var avgHours = AvgHoursForClass.First(x => x.FkSchedule == slotInfo.FkSchedule);
                foreach(var slot in availableSlots) 
                {
                    
                    if (IsClassHaveTwoSameSubjectsInDay(slotInfo, slot.Key) || avgHours.MaxLessonADay <= slot.Key.time)
                    {
                        continue;
                    }
                    if (IsTeacherFree(slotInfo, slot.Key) && IsPreviousSlotEmpty(slot.Key))
                    {
                        if (IsDayHoursNormal(avgHours.AvgHours, slot.Key))
                        {
                            ClassSlotsAvailability[slot.Key] = false;
                            var teacherSlotKey = (slot.Key.day, slot.Key.time, slotInfo.FkTeacher);
                            TeacherSlotsAvailability[teacherSlotKey] = false;
                            AddSlotToTakenSlots(slotInfo, (slot.Key.day, slot.Key.time));
                            i++;
                            break;
                        }
                    }
                                       
                }
                if(i == startingI) 
                {
                    otherLessons.Add(slotInfo);
                    i++;
                }
            }
            i = 0;
            //OTHER LESSONS
            otherLessons = otherLessons.OrderBy(x => random.Next()).ToList();
            i = 0;
            while (i < otherLessons.Count)
            {
                int startingI = i;
                var slotInfo = otherLessons[i];
                var availableSlots = ClassSlotsAvailability.Where(x => x.Value == true
                    && x.Key.clas == slotInfo.FkSchedule).ToList();
                var avgHours = AvgHoursForClass.First(x => x.FkSchedule == slotInfo.FkSchedule);
                foreach (var slot in availableSlots)
                {

                    if (IsClassHaveTwoSameSubjectsInDay(slotInfo, slot.Key) || avgHours.MaxLessonADay <= slot.Key.time)
                    {
                        continue;
                    }
                    if (IsTeacherFree(slotInfo, slot.Key) && IsPreviousSlotEmpty(slot.Key))
                    {
                        if (IsDayHoursNormal(avgHours.AvgHours, slot.Key))
                        {
                            ClassSlotsAvailability[slot.Key] = false;
                            var teacherSlotKey = (slot.Key.day, slot.Key.time, slotInfo.FkTeacher);
                            TeacherSlotsAvailability[teacherSlotKey] = false;
                            AddSlotToTakenSlots(slotInfo, (slot.Key.day, slot.Key.time));
                            i++;
                            break;
                        }
                    }

                }
                if (i == startingI)
                {
                    restOfTHeLessons.Add(slotInfo);
                    i++;
                }
            }
            i = 0;
            var finalLessons = new List<ScheduleSubjectToClass>();
            while (i < restOfTHeLessons.Count)
            {
                int startingI = i;
                var slotInfo = restOfTHeLessons[i];
                var availableSlots = ClassSlotsAvailability.Where(x => x.Value == true
                    && x.Key.clas == slotInfo.FkSchedule).ToList();
                var avgHours = AvgHoursForClass.First(x => x.FkSchedule == slotInfo.FkSchedule);
                foreach (var slot in availableSlots)
                {

                    if (IsClassHaveTwoSameSubjectsInDay(slotInfo, slot.Key) || avgHours.MaxLessonADay <= slot.Key.time)
                    {
                        continue;
                    }
                    if (IsTeacherFree(slotInfo, slot.Key) && IsPreviousSlotEmpty(slot.Key))
                    {

                        ClassSlotsAvailability[slot.Key] = false;
                        var teacherSlotKey = (slot.Key.day, slot.Key.time, slotInfo.FkTeacher);
                        TeacherSlotsAvailability[teacherSlotKey] = false;
                        AddSlotToTakenSlots(slotInfo, (slot.Key.day, slot.Key.time));
                        i++;
                        break;
                        
                    }

                }
                if (i == startingI)
                {
                    finalLessons.Add(slotInfo);
                    i++;
                }
            }
            i = 0;
            //FINAL LOOP
            while (i < finalLessons.Count)
            {
                int startingI = i;
                var slotInfo = finalLessons[i];
                var availableSlots = ClassSlotsAvailability.Where(x => x.Value == true
                    && x.Key.clas == slotInfo.FkSchedule).ToList();
                var avgHours = AvgHoursForClass.First(x => x.FkSchedule == slotInfo.FkSchedule);
                foreach (var slot in availableSlots)
                {
                    if (IsClassHaveTwoSameSubjectsInDay(slotInfo, slot.Key))
                    {
                        continue;
                    }
                    if (IsTeacherFree(slotInfo, slot.Key) && IsPreviousSlotEmpty(slot.Key))
                    {
                        ClassSlotsAvailability[slot.Key] = false;
                        var teacherSlotKey = (slot.Key.day, slot.Key.time, slotInfo.FkTeacher);
                        TeacherSlotsAvailability[teacherSlotKey] = false;
                        AddSlotToTakenSlots(slotInfo, (slot.Key.day, slot.Key.time));
                        i++;
                        break;

                    }

                }
                if (i == startingI)
                {
                    i++;
                }
            }
            OnSchedulingCompleted();
            
        }

        private bool IsClassHaveTwoSameSubjectsInDay(ScheduleSubjectToClass sc, (int day, int time, int clas) clasKey)
        {
            var subjectCount = TakenSlots.Where(x=>x.SubjectToClassId == sc.Id && x.DayId == clasKey.day).Count();
            if(subjectCount < 2) 
            {
                return false;
            }
            else 
            {
                return true;
            }
        }

        private void CalculateAvgHoursForClasses(List<ScheduleSubjectToClass> subjectsToClass) 
        {
            foreach (var schedule in SchedulesInfo) 
            {
                double sumOfHours = subjectsToClass.Where(x => x.FkSchedule == schedule.ScheduleId).Sum(x => x.Hours);
                double hoursForClass;
                double divideRest = 0;
                if (sumOfHours <= 5) 
                {
                    hoursForClass = sumOfHours;
                }
                else 
                {
                    hoursForClass = Math.Round(sumOfHours / 5);//Add condition for avg hours because constraint for avg hours is too strong
                    divideRest = sumOfHours % 5;
                }
                AvgHoursForClass avg = new AvgHoursForClass
                {
                    FkSchedule = schedule.ScheduleId,
                    AvgHours = hoursForClass,
                    DivideRest = divideRest,
                    MaxLessonADay = (int)hoursForClass + 1
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
            bool isteacherFree = TeacherSlotsAvailability[teacherKey];
            return isteacherFree;
        }
        private bool IsPreviousSlotEmpty((int day, int time, int clas) clasKey)
        {
            if (clasKey.time == 1)
                return true;
            var previousLesson = ClassSlotsAvailability.Where(x => x.Key.day == clasKey.day && x.Key.clas == clasKey.clas
                && x.Value == false).OrderByDescending(x => x.Key.time).FirstOrDefault();
            int sub = clasKey.time - previousLesson.Key.time;
            if(sub > 1) 
            {
                return false;
            }
            else 
            {
                return true;
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
