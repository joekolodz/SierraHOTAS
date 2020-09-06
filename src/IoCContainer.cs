using Autofac;
using SierraHOTAS.ViewModels;
using System.Windows.Threading;
using SierraHOTAS.Models;

namespace SierraHOTAS
{
    public class IoCContainer
    {
        public static IContainer GetContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ActionCatalogViewModel>();
            builder.RegisterType<HOTASCollection>().As<IHOTASCollection>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            builder.Register(c => Dispatcher.CurrentDispatcher).As<Dispatcher>();
            builder.RegisterType<HOTASCollectionViewModel>();

            return builder.Build();
        }

    }
}
