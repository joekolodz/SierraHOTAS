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
        private static IContainer _container;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _container = IoCContainer.GetContainer();

            Current.MainWindow = new MainWindow(_container.Resolve<HOTASCollectionViewModel>());
            Current.MainWindow.Show();
        }

    }
}
