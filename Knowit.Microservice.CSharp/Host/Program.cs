using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Knowit.Grpc.Client;
using Knowit.Grpc.Correlation;
using Knowit.Grpc.Validation;
using Knowit.Grpc.Web;
using Knowit.Kestrel.ProtocolMultiplexing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectName;
using Repository;
using Serilog;
using Serilog.Events;
using Service;
using HostBuilder = Microsoft.Extensions.Hosting.Host;

namespace Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await ConfigureMigration(host);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => HostBuilder
            .CreateDefaultBuilder(args)
            .UseWindowsService()
            .UseSerilog(ConfigureSerilog)
            .ConfigureWebHostDefaults(web => web
                .Configure(Configure)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices)
                .ConfigureKestrel(ConfigureKestrel));

        private static void Configure(IApplicationBuilder app)
        {
            app.UseCorrelationId();
            app.UseSerilogRequestLogging();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseGrpcWeb();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapGrpcService<Service.Service>());
        }

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

        private static void ConfigureAppConfiguration(IConfigurationBuilder config)
        {
            var process = Process.GetCurrentProcess();
            var module = process.MainModule;
            if (module?.ModuleName == "dotnet") return;
            var path = Path.GetDirectoryName(module?.FileName);
            config.SetBasePath(path);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddConfigurableGrpcClient<Other.Api.Core.CoreClient>();
            services.AddValidator<EchoRequestValidator>();

            services.AddCorrelationId();
            services.AddGrpcWeb();
            services.AddGrpc(options => options.AddValidationInterceptor());
            
            services.AddDbContext<Database>(ConfigureDbContext);
        }

        private static void ConfigureDbContext(IServiceProvider provider, DbContextOptionsBuilder options)
        {
            var logger = provider.GetRequiredService<ILogger<Program>>();
            var connectionString = provider
                .GetRequiredService<IConfiguration>()
                .GetConnectionString("Database");
            var databaseOptions = provider.GetRequiredService<IOptionsMonitor<DatabaseOptions>>().CurrentValue;
            var environment = provider.GetRequiredService<IWebHostEnvironment>();

            if (string.IsNullOrEmpty(connectionString))
            {
                var path = Path.IsPathRooted(databaseOptions.Path)
                    ? Path.Combine(databaseOptions.Path, "projectname.db")
                    : Path.Combine(environment.ContentRootPath, databaseOptions.Path, "projectname.db");
                path = Path.GetFullPath(path);
                options.UseSqlite($"Data Source={path}");

                logger.LogInformation(
                    "No database connection string provided." +
                    "Using local SQLite database at '{Path}'.", path);
            }
            else
            {
                options.UseSqlServer(connectionString);

                var builder = new DbConnectionStringBuilder {ConnectionString = connectionString};
                builder.Remove("password");
                logger.LogInformation(
                    "Connecting to database using connection: {ConnectionString}",
                    builder.ConnectionString);
            }
        }

        private static async Task ConfigureMigration(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var databaseOptions = scope.ServiceProvider
                .GetRequiredService<IOptionsMonitor<DatabaseOptions>>()
                .CurrentValue;
            var database = scope.ServiceProvider.GetRequiredService<Database>();

            if (databaseOptions.Migrate)
            {
                logger.LogInformation("Migrating databases");
                await database.Database.MigrateAsync();
                logger.LogInformation("Migration complete");
                if (databaseOptions.OnlyMigrate) Environment.Exit(0);
            }
            else
            {
                logger.LogInformation("Skipping database migrations");
            }
        }

        private static void ConfigureKestrel(KestrelServerOptions kestrel)
        {
            kestrel.ConfigureEndpointDefaults(options => options.UseProtocolMultiplexing());
        }
    }
}