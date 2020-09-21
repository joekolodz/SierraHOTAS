using Autofac;
using SierraHOTAS.ViewModels;
using System.Windows;

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
            
            Current.MainWindow = new MainWindow(container.Resolve<HOTASCollectionViewModel>());
            Current.MainWindow.Show();
        }

    }
}
