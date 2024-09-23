using Schedule.Services;

namespace Schedule.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public HomeViewModel(UserSessionService? sessionService) 
        {
            base.SessionService = sessionService;
        }
        public HomeViewModel() { }
    }
}
