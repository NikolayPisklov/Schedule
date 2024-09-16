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
        public UserSessionService? SesssionService 
        {
            get => _sessionService;
            set 
            {
                if(_sessionService is null) 
                {
                    _sessionService = value;
                    RaisePropertyChange();
                }               
            }
        }
        public DelegateCommand? SelectViewModelCommand { get; }
        private ViewModelBase? _selectedViewModel = new LoginViewModel(new LoginDataProvider());
        private UserSessionService? _sessionService = null;
        public MainViewModel()
        {
            SelectViewModelCommand = new DelegateCommand(SelectView);
            Messanger.Instance.ViewChanged += OnViewChanged;
            Messanger.Instance.SessionCreation += OnSessionCreation;
        }   
        public async override Task LoadAsync() 
        {
            if(SelectedViewModel is not null) 
            {
                await SelectedViewModel.LoadAsync();
            }
        }
        public async void SelectView(object? parameter) 
        {
            SelectedViewModel = parameter as ViewModelBase;
            await LoadAsync();
        }

        private async void OnViewChanged(object newViewModel)
        {
            SelectedViewModel = newViewModel as ViewModelBase;
            await LoadAsync();
        }
        private async void OnSessionCreation(object userSessionService)
        {
            SesssionService = userSessionService as UserSessionService;
            await LoadAsync();
        }
    }
}
