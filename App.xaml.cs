using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Schedule.DataProviders;
using Schedule.ViewModels;
using Schedule.Views;

namespace Schedule
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App() 
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider(); 
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddTransient<MainWindow>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<ClassesEditViewModel>();
            services.AddTransient<TeacherEditViewModel>();
            services.AddTransient<ScheduleViewModel>();
            services.AddTransient<SubjectToClassViewModel>();

            services.AddTransient<DataProviderBase>();
            services.AddTransient<ILoginDataProvider, LoginDataProvider>();
            services.AddTransient<ISubjectDataProvider, SubjectDataProvider>();
            services.AddTransient<IClassesDataProvider, ClassesDataProvider>();
            services.AddTransient<ITeacherDataProvider, TeacherDataProvider>();
            services.AddTransient<ITeacherSubjectDataProvider, TeacherSubjectDataProvider>();
            services.AddTransient<IScheduleDataProvider, ScheduleDataProvider>();
            services.AddTransient<ISubjectToClassDataProvider, SubjectToClassDataProvider>();
        }

        protected override void OnStartup(StartupEventArgs e) 
        {
            base.OnStartup(e);

            var mainWIndow = _serviceProvider.GetService<MainWindow>();
            mainWIndow?.Show();
            // Отображение консоли
            ConsoleHelper.ShowConsole();
            Console.WriteLine("Добро пожаловать в программу!");
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Закрытие консоли при завершении программы
            ConsoleHelper.CloseConsole();
        }
    }
}