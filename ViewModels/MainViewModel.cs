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
        private ViewModelBase? _selectedViewModel = new LoginViewModel(new LoginDataProvider());
        public MainViewModel()
        {
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

        private async void OnViewChanged(object newViewModel)
        {
            SelectedViewModel = newViewModel as ViewModelBase;
            await LoadAsync();
        }
        private async void OnSessionCreation(object userSessionService)
        {
            base.SessionService = userSessionService as UserSessionService;
            await LoadAsync();
        }
    }
}
