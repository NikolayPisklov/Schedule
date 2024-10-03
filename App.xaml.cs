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

            services.AddTransient<DataProviderBase>();
            services.AddTransient<ILoginDataProvider, LoginDataProvider>();
            services.AddTransient<IClassesDataProvider, ClassesDataProvider>();
        }

        protected override void OnStartup(StartupEventArgs e) 
        {
            base.OnStartup(e);

            var mainWIndow = _serviceProvider.GetService<MainWindow>();
            mainWIndow?.Show();
        }
    }
}