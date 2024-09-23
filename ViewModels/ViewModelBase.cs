using Schedule.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Schedule.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public UserSessionService? SessionService
        {
            get
            {
                return _sessionService;
            }
            set
            {
                if (_sessionService is null)
                {
                    _sessionService = value;
                    RaisePropertyChange();
                }
            }
        }
        private UserSessionService? _sessionService = null;
        virtual protected void RaisePropertyChange([CallerMemberName] string? propertyName = null) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public virtual Task LoadAsync() => Task.CompletedTask;  
    }
}
