using Autofac;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;
using SierraHOTAS.Win32;

namespace SierraHOTAS
{
    public class IoCContainer
    {
        public static  AutofacContractResolver ContractResolver { get; set; }

        public static IContainer GetContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            builder.RegisterType<DispatcherFactory>();
            builder.RegisterType<DirectInputFactory>();
            builder.RegisterType<JoystickFactory>();
            builder.RegisterType<MediaPlayerFactory>();
            builder.RegisterType<HOTASDeviceFactory>();
            builder.RegisterType<DeviceViewModelFactory>();
            builder.RegisterType<HOTASQueueFactory>();
            builder.RegisterType<KeyboardWrapper>().As<IKeyboard>();
            builder.RegisterType<FileDialogFactory>().As<FileDialogFactory>();

            builder.RegisterType<HOTASCollectionViewModel>();
            builder.RegisterType<QuickProfilePanelViewModel>();
            builder.RegisterType<ActionCatalog>();
            
            builder.RegisterType<HOTASCollection>().As<IHOTASCollection>();
            builder.RegisterType<HOTASQueue>().As<IHOTASQueue>();
            
            builder.RegisterType<FileIO>().As<IFileIO>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            var container =  builder.Build();

            ContractResolver = new AutofacContractResolver(container);

            return container;
        }
    }
}
