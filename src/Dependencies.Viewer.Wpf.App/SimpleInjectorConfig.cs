﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Dependencies.Analyser.Base;
using Dependencies.Viewer.Wpf.App.Layouts;
using Dependencies.Viewer.Wpf.Controls;
using Dragablz;
using SimpleInjector;

namespace Dependencies.Viewer.Wpf.App
{
    internal static class SimpleInjectorConfig
    {
        public static Container Container { get; private set; }

        public static void Config()
        {
            Container = new Container();

            Container.Register<IInterTabClient, KeepOneInterLayoutClient>();
            Container.Register<ISettingProvider, SettingProvider>(Lifestyle.Singleton);
            Container.Register(typeof(IServiceFactory<>), typeof(SimpleInjectorServiceFactory<>));
            Container.Register<AnalyserProvider>(Lifestyle.Singleton);
            RegisterAnalyser(Container);

            Container.Verify();
        }

        private static void RegisterAnalyser(Container container)
        {
            string pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

            var files = new DirectoryInfo(pluginDirectory).GetFiles("Dependencies.Analyser*", SearchOption.AllDirectories);

            var pluginAssemblies = files.Where(x => x.Extension == ".dll").Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x.FullName)));

            container.Collection.Register<IAssemblyAnalyserFactory>(pluginAssemblies);
            container.Collection.Register<IAssemblyAnalyser>(pluginAssemblies);
        }
    }
}