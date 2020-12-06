﻿using System;
using System.Windows;
using Dependencies.Viewer.Wpf.Controls.Base;
using Dependencies.Viewer.Wpf.IoC;
using Dependencies.Viewer.Wpf.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Dependencies.Viewer.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            var config = new ConfigurationBuilder()
                      .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .Build();

            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

            SimpleInjectorConfig.Config(config);
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var logger = SimpleInjectorConfig.Container.GetInstance<ILogger<App>>();

            logger.LogTrace($"Dependencies Viewer v{GetType().Assembly.GetName().Version} started");

            ConfigureTheme();

            string? filename = null;

            if (e.Args.Length == 1) // make sure an argument is passed
                filename = e.Args[0];

            MainWindow = new MainWindow(filename);
            MainWindow.Show();
        }

        private static void ConfigureTheme()
        {
            var themeManager = SimpleInjectorConfig.Container.GetInstance<ThemeManager>();

            themeManager.AddTheme("Light", new Uri($"pack://application:,,,/Dependencies Viewer;component/Themes/LightTheme.xaml"));
            themeManager.AddTheme("Dark", new Uri($"pack://application:,,,/Dependencies Viewer;component/Themes/DarkTheme.xaml"));

            themeManager.ApplyTheme(Settings.Default.SelectedTheme);
        }
    }
}
