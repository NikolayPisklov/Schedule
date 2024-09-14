using Schedule.Command;
using Schedule.DataProviders;
using System.Windows;

namespace Schedule.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public string? Login { get => _login; set => _login = value; }
        public string? Password { get => _password; set => _password = value; }

        private ILoginDataProvider _loginDataProvider;

        public DelegateCommand SignInCommand { get; }

        private string? _login;
        private string? _password;
        public LoginViewModel(ILoginDataProvider loginDataProvider)
        {
            _loginDataProvider = loginDataProvider;
            SignInCommand = new DelegateCommand(SignIn);
        }
        private void SignIn(object? parameter) 
        {
            if(Login is not null && Password is not null) 
            {
                var user = _loginDataProvider.GetUserByLoginDetailsAsync(Login, Password);
                MessageBox.Show($"{user.Name}, добрий день!");
            }
            else 
            {
                MessageBox.Show("Введіть логін та пароль!");
            }
        }
    }
}
