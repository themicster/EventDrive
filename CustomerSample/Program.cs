using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EventDriveCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventDriveCore.Services;

namespace ConsoleSample
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            SetupConfiguration();
            string eventStoreConnectionString = Configuration["EventDrive:EventStoreConnectionString"];

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            DefaultMessageBroker mb = (DefaultMessageBroker)serviceProvider.GetService<IMessageBroker>();
            mb.UseEventStore(eventStoreConnectionString);
            
            mb.Subscribe("GetCustomerNameQuery", GetCustomerNameQuery.OnGetCustomerNameQuery);

            Console.WriteLine("Starting EventBroker");
            mb.StartInboundQueue();
            mb.StartOutboundQueue();

            Console.ReadLine();
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            services.UseDefaultMessageBroker();
        }
        public static void SetupConfiguration()
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");

            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }

            Configuration = builder.Build();
        }

    }
}
