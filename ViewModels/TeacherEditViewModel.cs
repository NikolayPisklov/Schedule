using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Models;
using Schedule.Services;
using System.Collections.ObjectModel;
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
                if (_selectedTeacher is not null)
                    EditedTeacherName = _selectedTeacher.FullName;
                RaisePropertyChange();
                RaisePropertyChange(nameof(IsEditButtonVisible));
                RaisePropertyChange(nameof(IsDeleteButtonVisible));
                RaisePropertyChange(nameof(IsTeacherSelected));
                RaisePropertyChange(nameof(IsSubjectListVisible));
                RaisePropertyChange(nameof(EditedTeacherName));
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
        public string? NewTeachersName { get; set; }
        public string? EditedTeacherName { get; set; }
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
        public bool IsTeacherSelected =>  SelectedTeacher is not null; 
            
        
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
        public bool IsAddingFormVisible
        {
            get => _isAddingFormVisible;
            set
            {
                _isAddingFormVisible = value;
                RaisePropertyChange();
            }
        }
        public bool IsEditFormVisible
        {
            get => _isEditFormVisible;
            set
            {
                _isEditFormVisible = value;
                RaisePropertyChange();
            }
        }
        public DelegateCommand ShowAddingFormCommand { get; }
        public DelegateCommand AttachSubjectToTeacherCommand { get; }
        public DelegateCommand DeattachTeachersSubjectCommand { get; }
        public DelegateCommand ShowEditFormCommand { get; }
        public DelegateCommand ShowMainFormCommand { get; }
        public DelegateCommand DeleteTeacherCommand { get; }
        public DelegateCommand AddTeacherCommand { get; }
        public DelegateCommand BackToMainFormCommand { get; }
        public DelegateCommand EditTeacherCommand { get; }
        private Teacher? _selectedTeacher;
        private readonly ITeacherDataProvider _teacherDataProvider;
        private readonly ISubjectDataProvider _subjectDataProvider;
        private readonly ITeacherSubjectDataProvider _teacherSubjectDataProvider = new TeacherSubjectDataProvider();
        private bool _isListTeachersVisible;
        private Subject? _selectedSubject;
        private Subject? _selectedSubjectForAttaching;
        private bool _isSubjectListVisible = false;
        private bool _isAddingFormVisible;
        private bool _isEditFormVisible;

        public TeacherEditViewModel(ITeacherDataProvider teacherDataProvider,
            ISubjectDataProvider subjectDataProvider)
        {
            _teacherDataProvider = teacherDataProvider;
            _subjectDataProvider = subjectDataProvider;
            ShowAddingFormCommand = new DelegateCommand(ShowAddingForm);
            ShowEditFormCommand = new DelegateCommand(ShowEditForm);
            DeleteTeacherCommand = new DelegateCommand(DeleteTeacher);
            AttachSubjectToTeacherCommand = new DelegateCommand(AttachSubjectToTeacher);
            DeattachTeachersSubjectCommand = new DelegateCommand(DeattachTeachersSubject);
            ShowMainFormCommand = new DelegateCommand(ShowMainForm);
            AddTeacherCommand = new DelegateCommand(AddTeacher);
            EditTeacherCommand = new DelegateCommand(EditTeacher);
            BackToMainFormCommand = new DelegateCommand(ShowMainForm);
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

        //Editing teacher's sunbects functionality
        private async void LoadTeachersSubjectsAsync(object? sender, EventArgs e)
        {
            TeachersSubjects.Clear();
            if (SelectedTeacher is not null)
            {
                var subjects = await _teacherSubjectDataProvider.GetSubjectsForTeacherAsync(SelectedTeacher.Id);
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
                await _teacherSubjectDataProvider.InsertTeacherSubjectAsync(teacherSubject);
                MessageBox.Show("Предмет успішно закріплено до вчителя!", "Операція успішна", _cancelButton, _iconSuccess);
                LoadTeachersSubjectsAsync(this, EventArgs.Empty);
                RaisePropertyChange(nameof(TeachersSubjects));
                RaisePropertyChange(nameof(AllSubjects));
            }

        }
        private async void DeattachTeachersSubject(object? obj)
        {
            if (SelectedTeacher is not null && SelectedSubject is not null)
            {
                   
                    TeacherSubject ts = await _teacherSubjectDataProvider.GetTeacherSubjectAsync(SelectedTeacher.Id,
                        SelectedSubject.Id);
                    await _teacherSubjectDataProvider.DeleteTeachersSubjectAsync(ts.Id);
                    MessageBox.Show("Предмет успішно відкріплено від вчителя!", "Операція успішна",
                        _cancelButton, _iconSuccess);
                    LoadTeachersSubjectsAsync(this, EventArgs.Empty);
                    RaisePropertyChange(nameof(TeachersSubjects));
                    RaisePropertyChange(nameof(AllSubjects));
               
            }
        }

        //Teacher functionality
        private async void DeleteTeacher(object? obj)
        {
            if (SelectedTeacher is not null)
            {
                
                await _teacherDataProvider.DeleteTeacherAndAttachedSubjectsAsync(SelectedTeacher.Id);
                Teachers.Remove(SelectedTeacher);
                MessageBox.Show("Вчителя успішно видалено з системи!", "Операція успішна",
                    _cancelButton, _iconSuccess);
                RaisePropertyChange(nameof(Teachers));
            }
        }
        private async void AddTeacher(object? obj) 
        {
            if (string.IsNullOrEmpty(NewTeachersName))
                MessageBox.Show("Будь ласка, введіть ім'я вчителя", "Помилка",
                            _cancelButton, _iconFail);
            else 
            {
                var newTeacher = new Teacher { FullName = NewTeachersName };
                await _teacherDataProvider.InsertTeacherAsync(newTeacher);
                MessageBox.Show("Вчителя успішно додано до системи!", "Операція успішна",
                        _cancelButton, _iconSuccess);
                newTeacher = await _teacherDataProvider.GetLatestAddedTeacherAsync();
                Teachers.Add(newTeacher);
                NewTeachersName = string.Empty;
            }
        }
        private async void EditTeacher(object? obj) 
        {
            if (string.IsNullOrEmpty(EditedTeacherName))
                MessageBox.Show("Будь ласка, введіть ім'я вчителя", "Помилка",
                            _cancelButton, _iconFail);
            else 
            {
                if (SelectedTeacher is not null) 
                {
                    var editedTeacher = new Teacher { Id = SelectedTeacher.Id, FullName = EditedTeacherName };
                    await _teacherDataProvider.UpdateTeacherAsync(editedTeacher);
                    MessageBox.Show("Вчителя успішно відредаговано!", "Операція успішна",
                        _cancelButton, _iconSuccess);
                    Teachers.Remove(SelectedTeacher);
                    Teachers.Add(editedTeacher);
                }
            }
        }
        private void ShowAddingForm(object? obj)
        {
            SelectedTeacher = null;
            IsListTeachersVisible = false;
            IsAddingFormVisible = true;
            IsEditFormVisible = false;
        }
        private void ShowEditForm(object? obj)
        {
            IsListTeachersVisible = false;
            IsAddingFormVisible = false;
            IsEditFormVisible = true;
        }
        private void ShowMainForm(object? obj) 
        {
            IsListTeachersVisible = true;
            IsAddingFormVisible = false;
            IsEditFormVisible = false;
        }
    }
}
