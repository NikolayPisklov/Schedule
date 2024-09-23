using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Schedule.ViewModels
{
    public class ValidationViewModelBase : ViewModelBase, INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();
        public bool HasErrors => _errorsByPropertyName.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            return propertyName is not null && _errorsByPropertyName.ContainsKey(propertyName)
                ? _errorsByPropertyName[propertyName] : Enumerable.Empty<string>();
        }
        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs args) 
        {
            ErrorsChanged?.Invoke(this, args);
        }
        protected void AddError(string error, 
            [CallerMemberNameAttribute] string? propertyName = null) 
        {
            if(propertyName is null) 
            {
                return;
            }
            if (!_errorsByPropertyName.ContainsKey(propertyName)) 
            {
                _errorsByPropertyName[propertyName] = new List<string>();
            }
            if (!_errorsByPropertyName[propertyName].Contains(error)) 
            {
                _errorsByPropertyName[propertyName].Add(error);
                OnErrorsChanged(new DataErrorsChangedEventArgs(propertyName));
                RaisePropertyChange(nameof(HasErrors));
            }
        }
        protected void ClearErrors([CallerMemberNameAttribute] string? propertyName = null) 
        {
            if (propertyName is null)
            {
                return;
            }
            if (_errorsByPropertyName.ContainsKey(propertyName)) 
            {
                _errorsByPropertyName.Remove(propertyName);
                OnErrorsChanged(new DataErrorsChangedEventArgs(propertyName));
                RaisePropertyChange(nameof(HasErrors));
            }
        }
    }
}
