using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Schedule.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        virtual protected void RaisePropertyChange([CallerMemberName] string? propertyName = null) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public virtual Task LoadAsync() => Task.CompletedTask;  
    }
}
