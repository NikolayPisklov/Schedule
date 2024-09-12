using Schedule.Command;

namespace Schedule.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public LoginViewModel? LoginViewModel { get; }
        public ViewModelBase? SelectedViewModel
        {
            get => _selectedViewModel;
            set 
            {
                _selectedViewModel = value;
                RaisePropertyChange();
            }
        }
        public DelegateCommand? SelectViewModelCommand { get; }
        private ViewModelBase? _selectedViewModel;

        public MainViewModel(LoginViewModel loginViewModel)
        {
            LoginViewModel = loginViewModel;
            SelectViewModelCommand = new DelegateCommand(SelectView);
            _selectedViewModel = LoginViewModel;
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
    }
}
