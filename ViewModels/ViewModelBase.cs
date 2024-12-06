using Schedule.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Schedule.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected readonly MessageBoxImage _iconSuccess = MessageBoxImage.Asterisk;
        protected readonly MessageBoxImage _iconFail = MessageBoxImage.Error;
        protected readonly MessageBoxButton _cancelButton = MessageBoxButton.OK;
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
