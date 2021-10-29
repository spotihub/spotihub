using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Incremental.Common.Configuration;
using Incremental.Common.Logging;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SpotiHub.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var assembly = Assembly.GetCallingAssembly().GetName().Name;
            
            try
            {
                var host = CreateHostBuilder(args).Build();
                
                if (Log.Logger is not null)
                {
                    Log.Information("Starting {Service}", assembly);
                }

                host.Run();
            }
            catch (Exception ex)
            {
                if (Log.Logger is not null)
                {
                    Log.Fatal(ex, "{Service} terminated unexpectedly", assembly);
                }
            }
            finally
            {
                if (Log.Logger is not null)
                {
                    Log.CloseAndFlush();
                }
            }
        }
        
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseLamar()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.AddCommonLogging();
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommonConfiguration();
                });
    }
}