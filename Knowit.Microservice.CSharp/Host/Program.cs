using System.Data.Common;
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
using Microsoft.Extensions.Logging;
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
        public static void Main(string[] args) => CreateHostBuilder(args)
            .Build()
            .Run();

        public static IHostBuilder CreateHostBuilder(string[] args) => HostBuilder
            .CreateDefaultBuilder(args)
            .UseSerilog((context, configuration) => configuration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext())
            .ConfigureWebHostDefaults(webBuilder => webBuilder
                .Configure(Configure)
                .ConfigureServices(ConfigureServices)
                .ConfigureKestrel(ConfigureKestrel));

        private static void Configure(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseGrpcWeb();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapGrpcService<Service.Service>());
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpcClientConfiguration<Other.OtherClient>();
            services.AddCorrelationId();
            services.AddGrpcWeb();
            services.AddGrpc(options =>
            {
                options.AddCorrelationId();
                options.AddValidationInterceptor();
            });
            services.AddValidator<EchoRequestValidator>();

            ConfigureDatabaseServices(services);
        }

        private static void ConfigureDatabaseServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetService<ILogger<Program>>();

            var connectionString = configuration.GetConnectionString("DatabaseConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogInformation("No database connection string provided. Using in memory database.");
                services.AddDbContext<Database>(options => options.UseInMemoryDatabase("PdfServer.DocumentManager"));
            }
            else
            {
                var connectionStringBuilder = new DbConnectionStringBuilder {ConnectionString = connectionString};
                connectionStringBuilder.Remove("password");

                logger.LogInformation(
                    "Connecting to database using connection: {ConnectionString}",
                    connectionStringBuilder.ConnectionString
                );
                services.AddDbContext<Database>(options => options.UseSqlServer(connectionString));
            }
        }

        private static void ConfigureKestrel(KestrelServerOptions kestrel)
        {
            kestrel.ConfigureEndpointDefaults(options => options.UseProtocolMultiplexing());
        }
    }
}
