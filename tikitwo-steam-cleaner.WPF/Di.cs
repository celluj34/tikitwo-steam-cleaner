﻿using Autofac;
using tikitwo_steam_cleaner.Application.Services;

namespace tikitwo_steam_cleaner.WPF
{
    internal class Di
    {
        private static IContainer _container;

        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public static void ConfigureContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<SteamFolderService>().As<ISteamFolderService>();
            containerBuilder.RegisterType<LogicalDriveService>().As<ILogicalDriveService>();

            _container = containerBuilder.Build();
        }
    }
}