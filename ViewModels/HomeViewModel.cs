using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Services;

namespace Schedule.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public DelegateCommand RedirectToClassesEditViewCommand { get; }
        public DelegateCommand RedirectToTeachersEditViewCommand { get; }
        public HomeViewModel(UserSessionService? sessionService) 
        {
            base.SessionService = sessionService;
            RedirectToClassesEditViewCommand = new DelegateCommand(RedirectToClassesEditView);
            RedirectToTeachersEditViewCommand = new DelegateCommand(RedirectToTeachersEditView);
        }
        public HomeViewModel()
        {
            RedirectToClassesEditViewCommand = new DelegateCommand(RedirectToClassesEditView);
            RedirectToClassesEditViewCommand = new DelegateCommand(RedirectToClassesEditView);
            RedirectToTeachersEditViewCommand = new DelegateCommand(RedirectToTeachersEditView);
        }
        private void RedirectToClassesEditView(object? parameter)
        {
            var classesEditVIewModel = new ClassesEditViewModel(new ClassesDataProvider());
            Messanger.Instance.ViewChangedSend(classesEditVIewModel);
        }
        private void RedirectToTeachersEditView(object? parameter)
        {
            var teacherEditViewModel = new TeacherEditViewModel(new TeacherDataProvider(),
                new SubjectDataProvider());
            Messanger.Instance.ViewChangedSend(teacherEditViewModel);
        }

        
    }
}
