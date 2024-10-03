using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace Schedule.ViewModels
{

    public class ClassesEditViewModel : ValidationViewModelBase
    {
        public ObservableCollection<Class> Classes { get; set; } = new ObservableCollection<Class>();
        public Class? SelectedClass 
        {
            get => _selectedClass;
            set 
            {
                _selectedClass = value;
                RaisePropertyChange();
                RaisePropertyChange(nameof(IsEditButtonVisible));
                RaisePropertyChange(nameof(IsDeleteButtonVisible));
            }
        }
        public bool IsEditButtonVisible => SelectedClass is not null;
        public bool IsDeleteButtonVisible => SelectedClass is not null;
        public bool IsMainFormVisible 
        {
            get => _isMainFormVisible;
            set 
            {
                _isMainFormVisible = value;
                RaisePropertyChange();
            } 
        } 
        public bool IsAddFormVisible 
        {
            get => _isAddFormVisible;
            set 
            {
                _isAddFormVisible = value;
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
        public bool IsClassListVisible
        {
            get => _isClassListVisible;
            set 
            {
                _isClassListVisible = value;
                RaisePropertyChange();
            }
        }
        public bool IsListEmptyMessageVisible 
        {
            get => _isListEmptyMessageVisible;
            set 
            {
                _isListEmptyMessageVisible = value;
                RaisePropertyChange();
            }
        }
        public string NewTitleForClass 
        {
            get => _newTitleForClass;
            set 
            {
                _newTitleForClass = value;
                RaisePropertyChange();
            }
        }
        public int? Year 
        {
            get => _year;
            set 
            {
                _year = value;
                RaisePropertyChange();
            } 
        }

        public DelegateCommand ShowAddingFormCommand { get; }
        public DelegateCommand ShowEditFormCommand { get; }
        public DelegateCommand BackToMainFormCommand { get; }
        public DelegateCommand DeleteClassCommand { get; }
        public DelegateCommand AddClassCommand { get; }
        public DelegateCommand EditClassCommand { get; }

        private readonly IClassesDataProvider _dataProvider;
        private Class? _selectedClass;
        private string _newTitleForClass = string.Empty;
        private bool _isMainFormVisible = true;
        private bool _isAddFormVisible = false;
        private bool _isEditFormVisible = false;
        private bool _isClassListVisible = true;
        private bool _isListEmptyMessageVisible;
        private int? _year = null;
        private readonly MessageBoxImage _iconSuccess = MessageBoxImage.Asterisk;
        private readonly MessageBoxImage _iconFail = MessageBoxImage.Error;
        private readonly MessageBoxButton _cancelButton = MessageBoxButton.OK;

        public ClassesEditViewModel(IClassesDataProvider dataProvider) 
        {
            _dataProvider = dataProvider;

            ShowAddingFormCommand = new DelegateCommand(ShowAddingForm);
            ShowEditFormCommand = new DelegateCommand(ShowEditForm);
            BackToMainFormCommand = new DelegateCommand(BackToMainForm);

            DeleteClassCommand = new DelegateCommand(DeleteClass);
            AddClassCommand = new DelegateCommand(InsertClass);
            EditClassCommand = new DelegateCommand(UpdateClass);
        }

        public async override Task LoadAsync() 
        {
            if (Classes.Any()) return;
            var classes = await _dataProvider.GetAllAsync();
            if(classes is not null) 
            {
                foreach( var c in classes) 
                {
                    Classes.Add(c);
                }
            }
            ClassesListVisibilityCheck();
        }

        private async void DeleteClass(object? obj)
        {
            if (SelectedClass is not null) 
            {
                if (await _dataProvider.CheckIfSlotForClassExistsAsync(SelectedClass.Id))
                {
                    MessageBox.Show("Клас використовується в розкладі!", "Помилка",
                    _cancelButton, _iconFail);
                }
                else 
                {
                    await _dataProvider.DeleteClassAsync(SelectedClass.Id);
                    Classes.Remove(SelectedClass);
                    SelectedClass = null;
                    ClassesListVisibilityCheck();
                    MessageBox.Show("Клас успішнo видалено з системи!", "Операція успішна",
                        _cancelButton, _iconSuccess);                   
                }
                
            }
            
        }
        private async void InsertClass(object? obj)
        {
            if (string.IsNullOrEmpty(NewTitleForClass)) 
                MessageBox.Show("Будь ласка, введіть назву класу!", "Помилка", 
                    _cancelButton, _iconFail);
            else if (Year is null)
                MessageBox.Show("Будь ласка, введіть дату набору класу!", "Помилка",
                    _cancelButton, _iconFail);
            else
            {
                SelectedClass = null;
                Class newClass = new Class()
                {
                    Title = NewTitleForClass,
                    Year = Year
                };
                await _dataProvider.InsertClassAsync(newClass);
                AddNewClassToObservableCollection();
                MessageBox.Show("Клас успішнo додано до системи!", "Операція успішна",
                    _cancelButton, _iconSuccess);
            }           
        }
        private async void UpdateClass(object? obj)
        {
            if (string.IsNullOrEmpty(NewTitleForClass))
                MessageBox.Show("Будь ласка, введіть назву класу!", "Помилка",
                    _cancelButton, _iconFail);
            else if (Year is null)
                MessageBox.Show("Будь ласка, введіть дату набору класу!", "Помилка",
                    _cancelButton, _iconFail);
            else if(SelectedClass is not null)
            {
                await _dataProvider.UpdateClassAsync(NewTitleForClass, (int)Year, SelectedClass.Id);
                UpdateClassObservableInObservableCollection(NewTitleForClass, (int)Year, SelectedClass.Id);
                MessageBox.Show("Інформацію про клас успішно оновлено!", "Операція успішна",
                    _cancelButton, _iconSuccess);
            }
        }
        private void ShowEditForm(object? obj)
        {
            if (SelectedClass is not null) 
            {
                NewTitleForClass = SelectedClass.Title;
                Year = SelectedClass.Year;
                IsListEmptyMessageVisible = false;
                IsMainFormVisible = false;
                IsEditFormVisible = true;
                IsClassListVisible = false;
            }           
        }
        private void ShowAddingForm(object? obj)
        {
            NewTitleForClass = string.Empty;
            Year = null;
            IsListEmptyMessageVisible = false;
            IsAddFormVisible = true;
            IsMainFormVisible = false;
            IsClassListVisible = false;
        }
        private void BackToMainForm(object? obj)
        {           
            IsAddFormVisible = false;
            IsEditFormVisible = false;
            IsMainFormVisible = true;
            SelectedClass = null;
            ClassesListVisibilityCheck();
        }
        private async void AddNewClassToObservableCollection() 
        {
            Class latestClass = await _dataProvider.GetLatestAddedClass();
            if(latestClass is not null) 
            {
                Classes.Add(latestClass);
            }            
        }
        private void UpdateClassObservableInObservableCollection(string title, int year, int id) 
        {
            var found = Classes.First(x => x.Id == id);
            var updatedClass = new Class { Title = title, Year = year, Id = id };
            int i = Classes.IndexOf(found);
            Classes[i] = updatedClass;
        }
        private void ClassesListVisibilityCheck()
        {
            if (Classes.Count == 0)
            {
                IsClassListVisible = false;
                IsListEmptyMessageVisible = true;
            }
            else 
            {
                IsListEmptyMessageVisible = false;
                IsClassListVisible = true;
            }
        }
    }
}
