using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Services;

namespace Schedule.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public DelegateCommand RedirectToClassesEditViewCommand { get; }
        public HomeViewModel(UserSessionService? sessionService) 
        {
            base.SessionService = sessionService;
            RedirectToClassesEditViewCommand = new DelegateCommand(RedirectToClassesEditView);
        }
        public HomeViewModel()
        {
            RedirectToClassesEditViewCommand = new DelegateCommand(RedirectToClassesEditView);
        }
        private void RedirectToClassesEditView(object? parameter)
        {
            var classesEditVIewModel = new ClassesEditViewModel(new ClassesDataProvider());
            Messanger.Instance.ViewChangedSend(classesEditVIewModel);
        }

        
    }
}
