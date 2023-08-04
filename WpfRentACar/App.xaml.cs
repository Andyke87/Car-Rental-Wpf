using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace RentACar
{
    public partial class App : Application
    {
        private IConfiguration? configuration;
        private ServiceProvider? serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ConfigureServices();
        }
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Registreer de MainWindow als service
            services.AddSingleton<MainWindow>();

            configuration = new ConfigurationBuilder()
                .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Registreer de IConfiguration-service
            services.AddSingleton<IConfiguration>(configuration);

            // Voeg andere benodigde services toe
            serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<IConfiguration>();
        }
    }
}
