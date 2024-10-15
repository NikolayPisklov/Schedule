using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Models;
using Schedule.Services;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Windows;

namespace Schedule.ViewModels
{
    public class TeacherEditViewModel : ViewModelBase
    {
        public ObservableCollection<Teacher> Teachers { get; set; } = new ObservableCollection<Teacher>();
        public ObservableCollection<Subject> TeachersSubjects { get; set; } = new ObservableCollection<Subject>();
        public ObservableCollection<Subject> AllSubjects { get; set; } = new ObservableCollection<Subject>();
        public event EventHandler? TeacherSelected;
        public Teacher? SelectedTeacher
        {
            get => _selectedTeacher;
            set
            {
                _selectedTeacher = value;
                RaisePropertyChange();
                RaisePropertyChange(nameof(IsEditButtonVisible));
                RaisePropertyChange(nameof(IsDeleteButtonVisible));
                RaisePropertyChange(nameof(IsTeacherSelected));
                RaisePropertyChange(nameof(IsSubjectListVisible));
                OnTeacherSelected();
            }
        }
        public Subject? SelectedSubject 
        { 
            get => _selectedSubject;
            set 
            {
                _selectedSubject = value;
                RaisePropertyChange();
                RaisePropertyChange(nameof(IsDeleteButtonSubjectVisible));

            } 
        }     
        public Subject? SelectedSubjectForAttaching 
        {
            get => _selectedSubjectForAttaching;
            set 
            {
                _selectedSubjectForAttaching = value;
                RaisePropertyChange();
                RaisePropertyChange(nameof(IsAddingAttachedSubjectButtonActive));
            } 
        }
        public bool IsEditButtonVisible => SelectedTeacher is not null;
        public bool IsSubjectListVisible 
        {
            get => _isSubjectListVisible;
            set 
            {
                _isSubjectListVisible = value;
                RaisePropertyChange();
            }
        }
        public bool IsDeleteButtonVisible => SelectedTeacher is not null;
        public bool IsTeacherSelected => SelectedTeacher is not null;
        public bool IsDeleteButtonSubjectVisible => SelectedSubject is not null;
        public bool IsAddingAttachedSubjectButtonActive => SelectedSubjectForAttaching is not null;
        public bool IsListTeachersVisible
        {
            get => _isListTeachersVisible;
            set 
            {
                _isListTeachersVisible = value;
                RaisePropertyChange();
            }
        }
        public DelegateCommand ShowAddingFormCommand { get;}
        public DelegateCommand AttachSubjectToTeacherCommand { get;}
        public DelegateCommand DeattachTeachersSubjectCommand { get;}
        public DelegateCommand ShowEditFormCommand { get;}
        public DelegateCommand DeleteClassCommand { get;}
        private Teacher? _selectedTeacher;
        private ITeacherDataProvider _teacherDataProvider;
        private readonly ISubjectDataProvider _subjectDataProvider;
        private bool _isListTeachersVisible;
        private Subject? _selectedSubject;
        private Subject? _selectedSubjectForAttaching;
        private bool _isSubjectListVisible = false;
        private readonly MessageBoxImage _iconSuccess = MessageBoxImage.Asterisk;
        private readonly MessageBoxImage _iconFail = MessageBoxImage.Error;
        private readonly MessageBoxButton _cancelButton = MessageBoxButton.OK;

        public TeacherEditViewModel(ITeacherDataProvider teacherDataProvider,
            ISubjectDataProvider subjectDataProvider) 
        {
            _teacherDataProvider = teacherDataProvider;
            _subjectDataProvider = subjectDataProvider;
            ShowAddingFormCommand = new DelegateCommand(ShowAddingForm);
            ShowEditFormCommand = new DelegateCommand(ShowEditForm);
            DeleteClassCommand = new DelegateCommand(DeleteClass);
            AttachSubjectToTeacherCommand = new DelegateCommand(AttachSubjectToTeacher);
            DeattachTeachersSubjectCommand = new DelegateCommand(DeattachTeachersSubject);
            TeacherSelected += LoadTeachersSubjectsAsync;
        }
        public async override Task LoadAsync()
        {
            if (Teachers.Any()) return;
            var teachers = await _teacherDataProvider.GetAllTeachersAsync();
            if (teachers is not null)
            {
                foreach (var t in teachers)
                {
                    Teachers.Add(t);
                }
            }
            if (Teachers.Count() > 0) 
            {
                IsListTeachersVisible = true;
            }
        }
        private async void LoadTeachersSubjectsAsync(object? sender, EventArgs e) 
        {
            TeachersSubjects.Clear();
            if (SelectedTeacher is not null) 
            {
                var subjects = await _teacherDataProvider.GetTeachersSubjects(SelectedTeacher.Id);
                if (subjects is not null) 
                {
                    foreach (var subject in subjects) 
                    {
                        TeachersSubjects.Add(subject);
                    }
                    LoadAllSubjects();
                }
            }
            if (TeachersSubjects.Count == 0)
            {
                IsSubjectListVisible = false;
            }
            else 
            {
                IsSubjectListVisible = true;
            }
                
        }
        private async void LoadAllSubjects() 
        {
            AllSubjects.Clear();
            var subjects = await _subjectDataProvider.GetAllAsync();
            if (subjects is not null) 
            {
                var teachersSubjects = TeachersSubjects.ToList();
                var excluded = subjects.Except(teachersSubjects, new SubjectCompareService());
                foreach (var e in excluded) 
                {
                    AllSubjects.Add(e); 
                }
            }
        }
        private void OnTeacherSelected()
        {
            if (SelectedTeacher is not null)
            {
                TeacherSelected?.Invoke(this, EventArgs.Empty);
            }
        }
        private async void AttachSubjectToTeacher(object? obj)
        {
            if (SelectedTeacher is not null && SelectedSubjectForAttaching is not null) 
            {
                var teacherSubject = new TeacherSubject
                {
                    FkSubject = SelectedSubjectForAttaching.Id,
                    FkTeacher = SelectedTeacher.Id
                };
                await _teacherDataProvider.InsertSubjectForTeacherAsync(teacherSubject);
                MessageBox.Show("Предмет успішно закріплено до вчителя!", "Операція успішна", _cancelButton, _iconSuccess);
                LoadTeachersSubjectsAsync(this, EventArgs.Empty);
                RaisePropertyChange(nameof(TeachersSubjects));
                //LoadAllSubjects();
                RaisePropertyChange(nameof(AllSubjects));
            }
            
        }

        private async void DeattachTeachersSubject(object? obj)
        {
            if (SelectedTeacher is not null && SelectedSubject is not null)
            {
                if (await _teacherDataProvider.CheckIfSlotForTeacherSubjectExistsAsync
                    (SelectedTeacher.Id, SelectedSubject.Id))
                {
                    MessageBox.Show("Вчитель викладає даний предмет в одному з розкладів!", "Помилка",
                    _cancelButton, _iconFail);
                }
                else 
                {
                    TeacherSubject ts = await _teacherDataProvider.GetTeacherSubjectAsync(SelectedTeacher.Id, 
                        SelectedSubject.Id);
                    await _teacherDataProvider.DeleteTeachersSubjectAsync(ts.Id);
                    MessageBox.Show("Предмет успішно відкріплено від вчителя!", "Операція успішна",
                        _cancelButton, _iconSuccess);
                    LoadTeachersSubjectsAsync(this, EventArgs.Empty);
                    RaisePropertyChange(nameof(TeachersSubjects));
                    //LoadAllSubjects();
                    RaisePropertyChange(nameof(AllSubjects));
                }
            }
        }
        private void DeleteClass(object? obj)
        {
            throw new NotImplementedException();
        }

        private void ShowEditForm(object? obj)
        {
            throw new NotImplementedException();
        }

        private void ShowAddingForm(object? obj)
        {
            throw new NotImplementedException();
        }
    }
}
