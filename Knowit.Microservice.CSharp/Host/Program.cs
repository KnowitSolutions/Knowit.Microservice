using Knowit.Grpc.Correlation;
using Knowit.Grpc.Validation;
using Knowit.Grpc.Web;
using Knowit.Kestrel.ProtocolMultiplexing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Service;
using HostBuilder = Microsoft.Extensions.Hosting.Host;

namespace Host
{
    public class Program
    {
        public static void Main(string[] args) =>
            HostBuilder
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .Configure(Configure)
                    .ConfigureServices(ConfigureServices)
                    .ConfigureKestrel(ConfigureKestrel))
                .UseSerilog((context, configuration) => configuration
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext())
                .Build()
                .Run();

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
            // TODO: Add client config example
            services.AddCorrelationId();
            services.AddGrpcWeb();
            services.AddGrpc(options =>
            {
                options.AddCorrelationId();
                options.AddValidationInterceptor();
            });
            services.AddValidator<EchoRequestValidator>();
        }

        private static void ConfigureKestrel(KestrelServerOptions kestrel)
        {
            kestrel.ConfigureEndpointDefaults(options => options.UseProtocolMultiplexing());
        }
    }
}