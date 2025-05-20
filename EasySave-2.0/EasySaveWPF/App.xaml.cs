using EasySave.Localization;
using EasySave.Model;
using EasySaveWPF.ViewModels;
using EasySaveWPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace EasySaveWPF
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services in dependency order
            services.AddSingleton<ILocalizationService, JsonLocalizationService>();
            services.AddSingleton<BackupService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }
    }
}