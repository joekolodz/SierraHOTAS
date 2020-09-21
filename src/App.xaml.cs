using Autofac;
using SierraHOTAS.Views;
using SierraHOTAS.ViewModels;
using System.Windows;

namespace SierraHOTAS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool IsDebug { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var container = IoCContainer.GetContainer();

            Current.MainWindow = new MainWindow(container.Resolve<HOTASCollectionViewModel>());
            _ = new ViewService(Current.MainWindow, container.Resolve<IEventAggregator>());

            Current.MainWindow.Show();
        }

    }
}
