using Schedule.Command;
using Schedule.DataProviders;

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
        public DelegateCommand? SelectViewModelCommand { get; }
        private ViewModelBase? _selectedViewModel = new LoginViewModel(new LoginDataProvider());

        public MainViewModel()
        {
            SelectViewModelCommand = new DelegateCommand(SelectView);
            Messanger.Instance.ViewChanged += OnViewChanged;
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
            // Set the SelectedViewModel to the new view model
            SelectedViewModel = newViewModel as ViewModelBase;
            await LoadAsync();
        }
    }
}
