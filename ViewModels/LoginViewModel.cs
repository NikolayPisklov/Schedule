using Schedule.Command;
using Schedule.DataProviders;
using Schedule.Models;
using Schedule.Services;
using System.Windows;
using static Schedule.Services.UserSessionService;

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
        public void OnLoginSuccess() 
        {
            var homeViewModel = new HomeViewModel(base.SessionService);
            Messanger.Instance.ViewChangedSend(homeViewModel);
        }
        public void OnSessionCreation(User user) 
        {
            var userSession = new UserSessionService(user.Id, user.FkStatus,
                user.Name, user.Login, user.Email, GetUserStatusByFkStatus(user.FkStatus));
            base.SessionService = userSession;
            Messanger.Instance.SessionCreationSend(userSession);
        }
        private void SignIn(object? parameter) 
        {
            if (Login is not null && Password is not null)
            {
                User? user = _loginDataProvider.GetUserByLoginDetailsAsync(Login, Password);
                if (user is not null) 
                {
                    OnSessionCreation(user);
                    OnLoginSuccess();
                } 
                else MessageBox.Show("Невірний логін або пароль!", 
                    _loginFailCaption, _cancelButton, _icon);
            }
            else 
            {
                MessageBox.Show("Введіть логін та пароль!",
                    _loginFailCaption, _cancelButton, _icon);
            }
        }
        private UserStatus GetUserStatusByFkStatus(int fkStatus) 
        {
            switch (fkStatus) 
            {
                case 1:
                    return UserStatus.Admin;
                case 2:
                    return UserStatus.Teacher;
                case 3:
                    return UserStatus.Student;
            }
            return UserStatus.Unauthorized;
        }
    }
}
