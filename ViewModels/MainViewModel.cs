using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Services;

namespace Schedule.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ViewModelBase? SelectedViewModel
        {
            get => _selectedViewModel;
            set 
            {
                _selectedViewModel = value;
                RaisePropertyChange();
            }
        }
        public HomeViewModel HomeViewModel { get; } = new HomeViewModel();
        public ClassesEditViewModel ClassesEditViewModel { get; } = new ClassesEditViewModel(new ClassesDataProvider());
        public TeacherEditViewModel TeacherEditViewModel { get; } = new TeacherEditViewModel(new TeacherDataProvider(), new SubjectDataProvider());
        public DelegateCommand SelectMenuItemCommand { get; }
        private ViewModelBase? _selectedViewModel = new LoginViewModel(new LoginDataProvider());
        private bool _isMainMenuVisible;

        public bool IsMainMenuVisible 
        {
            get => _isMainMenuVisible;
            set 
            {
                _isMainMenuVisible = value;
                RaisePropertyChange();
            }
        }
        public MainViewModel()
        {
            SelectMenuItemCommand = new DelegateCommand(SelectMenuItem); 
            Messanger.Instance.ViewChanged += OnViewChanged;
            Messanger.Instance.SessionCreation += OnSessionCreation;
        }

        private void SelectMenuItem(object? parameter)
        {
            if (parameter is not null) 
            {
                Messanger.Instance.ViewChangedSend(parameter);
            }     
        }

        public async override Task LoadAsync() 
        {
            if(SelectedViewModel is not null) 
            {
                await SelectedViewModel.LoadAsync();
            }
        }

        private async void OnViewChanged(object newViewModel)
        {
            SelectedViewModel = newViewModel as ViewModelBase;
            await LoadAsync();
        }
        private async void OnSessionCreation(object userSessionService)
        {
            base.SessionService = userSessionService as UserSessionService;
            IsMainMenuVisible = true;
            await LoadAsync();
        }
    }
}
