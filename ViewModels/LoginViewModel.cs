using Schedule.Command;
using System.Windows;

namespace Schedule.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public string? Login { get; set; }
        public string? Password { get; set; }
        public DelegateCommand SignInCommand { get; }
        public LoginViewModel()
        {
            SignInCommand = new DelegateCommand(SignIn);
        }
        private void SignIn(object? parameter) 
        {
            MessageBox.Show($"Hello {Login}!\n {Password}");
        }
    }
}
