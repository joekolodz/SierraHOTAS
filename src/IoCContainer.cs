using Autofac;
using SierraHOTAS.Models;
using SierraHOTAS.ViewModels;

namespace SierraHOTAS
{
    public class IoCContainer
    {
        public static IContainer GetContainer()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterType<FileIO>().As<IFileIO>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            builder.RegisterType<HOTASCollectionViewModel>().As<HOTASCollectionViewModel>();

            return builder.Build();
        }

    }
}
