using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Models;
using System.Windows;

namespace Schedule.ViewModels
{
    public class LoginViewModel : ValidationViewModelBase
    {
        public string? Login { 
            get => _login; 
            set 
            { 
                _login = value;
                RaisePropertyChange();
                if (string.IsNullOrEmpty(_login)) 
                {
                    AddError("Будь ласка, введіть логін!");
                }
                else 
                {
                    ClearErrors();
                }
            } 
        }
        public string? Password
        {
            get => _password;
            set
            {
                _password = value;
                RaisePropertyChange();
                if (string.IsNullOrEmpty(_password))
                {
                    AddError("Будь ласка, введіть пароль!");
                }
                else
                {
                    ClearErrors();
                }
            }
        }

        

        public DelegateCommand SignInCommand { get; }

        private string? _login;
        private string? _password;
        private readonly ILoginDataProvider _loginDataProvider;
        private readonly MessageBoxImage _icon = MessageBoxImage.Exclamation;
        private readonly MessageBoxButton _cancelButton = MessageBoxButton.OK;
        private readonly string? _loginFailCaption = "Помилка при вході до системи";
        public LoginViewModel(ILoginDataProvider loginDataProvider)
        {
            _loginDataProvider = loginDataProvider;
            SignInCommand = new DelegateCommand(SignIn);
        }
        private void SignIn(object? parameter) 
        {
            if (Login is not null && Password is not null)
            {
                User? user = _loginDataProvider.GetUserByLoginDetailsAsync(Login, Password);
                if (user is not null) MessageBox.Show($"{user.Name}, добрий день!");
                else MessageBox.Show("Невірний логін або пароль!", _loginFailCaption, _cancelButton, _icon);
            }
            else 
            {
                MessageBox.Show("Введіть логін та пароль!", _loginFailCaption, _cancelButton, _icon);
            }
        }
    }
}
