using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bricks.Hometask.OperationTransformation.Console
{
    public static class Startup
    {
        public static IConfigurationRoot ConfigurationRoot { get; }
        public static IServiceProvider ServiceProvider { get; }

        static Startup()
        {
            ConfigurationRoot = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

            ServiceProvider = SetupServiceCollection(new ServiceCollection()).BuildServiceProvider();
        }

        static IServiceCollection SetupServiceCollection(IServiceCollection services)
        {

            services.AddSingleton<IConfiguration>(ConfigurationRoot);            

            return services;
        }
    }
}
