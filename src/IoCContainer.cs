using Autofac;
using SierraHOTAS.ViewModels;
using System.Windows.Threading;
using SierraHOTAS.Factories;
using SierraHOTAS.Models;

namespace SierraHOTAS
{
    public class IoCContainer
    {
        public static  AutofacContractResolver ContractResolver { get; set; }

        public static IContainer GetContainer()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => Dispatcher.CurrentDispatcher).As<Dispatcher>();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            builder.RegisterType<DirectInputFactory>();
            builder.RegisterType<JoystickFactory>();

            builder.RegisterType<ActionCatalogViewModel>();
            builder.RegisterType<HOTASCollection>().As<IHOTASCollection>();
            builder.RegisterType<HOTASQueue>().As<IHOTASQueue>();
            builder.RegisterType<HOTASQueueFactory>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            builder.RegisterType<HOTASCollectionViewModel>();

            var container =  builder.Build();

            ContractResolver = new AutofacContractResolver(container);

            return container;
        }
    }
}
