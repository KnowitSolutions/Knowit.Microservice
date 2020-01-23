using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Knowit.Kestrel.ProtocolMultiplexing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Options;
using Repository;
using Serilog;
using Serilog.Events;
using HostBuilder = Microsoft.Extensions.Hosting.Host;

namespace Host
{
	public class Program
	{
		private static ILogger _logger;

		public static async Task<int> Main(string[] args)
		{
			_logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateLogger();

			try
			{
				_logger.Information("Service is starting up.");
				_logger.Information("Initializing the host.");

				var host = CreateHostBuilder(args).Build();

				await MigrateDatabase(host);

				_logger.Information("Starting the host.");
				await host.RunAsync();
				return 0;
			}
			catch (Exception ex)
			{
				_logger.Fatal(ex, "Host terminated unexpectedly");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		private static async Task MigrateDatabase(IHost host)
		{
			using var scope = host.Services.CreateScope();
			var databaseOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

			if (databaseOptions.Migrate)
			{
				_logger.Information("Migrating database");
				var database = scope.ServiceProvider.GetRequiredService<Database>();
				await database.Database.MigrateAsync();
				_logger.Information("Migration complete");
				if (databaseOptions.OnlyMigrate) Environment.Exit(0);
			}
			else
			{
				_logger.Information("Data migration disabled. Skipping...");
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) => HostBuilder
			.CreateDefaultBuilder(args)
			.UseWindowsService()
			.UseSerilog(ConfigureSerilog)
			.ConfigureWebHostDefaults(webBuilder => webBuilder
				.ConfigureAppConfiguration(_ => ConfigureAppConfiguration(_, args))
				.ConfigureKestrel(ConfigureKestrel)
				.UseStartup<Startup>()
			);

		private static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration config)
		{
			config
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.ReadFrom.Configuration(context.Configuration)
				.Enrich.FromLogContext();

			if (WindowsServiceHelpers.IsWindowsService())
			{
				config.WriteTo.EventLog("Application", restrictedToMinimumLevel: LogEventLevel.Warning);
			}
		}

		private static void ConfigureAppConfiguration(IConfigurationBuilder config, string[] args)
		{
			void SetConfigurationBasePath()
			{
				var module = Process.GetCurrentProcess().MainModule;
				if (module?.ModuleName != "dotnet" && module?.ModuleName != "dotnet.exe")
				{
					config.SetBasePath(Path.GetDirectoryName(module?.FileName));
				}
			}

			SetConfigurationBasePath();
			config.AddEnvironmentVariables(prefix: "PROJECT_NAME_");
			config.AddCommandLine(args);
		}

		private static void ConfigureKestrel(KestrelServerOptions kestrel)
		{
			kestrel.ConfigureEndpointDefaults(options => options.UseProtocolMultiplexing());
		}
	}
}
