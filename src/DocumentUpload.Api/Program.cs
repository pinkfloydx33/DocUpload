using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DocumentUpload.Api
{
	public static class Program
	{
		public static async Task<int> Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();
			var env = host.Services.GetRequiredService<IWebHostEnvironment>();

			try
			{
				Log.Information("Starting {Application} - Environment: {Environment}", env.ApplicationName, env.EnvironmentName);

				using (host)
				{
					await host.RunAsync();
				}

				Log.Information("Application exiting...");

				return 0;
			}
			catch (OperationCanceledException oec)
			{
				Log.Error(oec, "Application startup was Canceled");
				return -2;
			}
			catch (Exception exception)
			{
				Log.Fatal(exception, "{Application} (Environment: {Environment}) terminated unexpectedly", env.ApplicationName, env.EnvironmentName);
				return -1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
			
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
				Host.CreateDefaultBuilder(args)
					.ConfigureWebHostDefaults(webBuilder =>
					{
						webBuilder.UseStartup<Startup>();
					})
					.UseSerilog(ConfigureSerilog)
					.UseConsoleLifetime();

		private static void ConfigureSerilog(HostBuilderContext hostContext, LoggerConfiguration logBuilder)
		{
			var env = hostContext.HostingEnvironment;

			logBuilder.ReadFrom.Configuration(hostContext.Configuration, "Serilog")
				.Enrich.WithProperty("Environment", env.EnvironmentName)
				.Enrich.WithProperty("Application", env.ApplicationName);

		}

	}
}
