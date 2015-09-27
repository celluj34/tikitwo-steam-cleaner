using Autofac;

namespace tikitwo_steam_cleaner.UAP
{
    internal class Di
    {
        private static IContainer _kernel;

        public static T Resolve<T>()
        {
            return _kernel.Resolve<T>();
        }

        public static void ConfigureContainer()
        {
            var kernel = new ContainerBuilder();

            _kernel = kernel.Build();
        }
    }
}