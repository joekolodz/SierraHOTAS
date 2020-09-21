using Autofac;
using SierraHOTAS.ViewModels;
using System.Windows;
using SierraHOTAS.Services;

namespace SierraHOTAS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var container = IoCContainer.GetContainer();
            
            var dialogService = new DialogService(container);

            Current.MainWindow = new MainWindow(container.Resolve<HOTASCollectionViewModel>());
            Current.MainWindow.Show();
        }

    }
}
