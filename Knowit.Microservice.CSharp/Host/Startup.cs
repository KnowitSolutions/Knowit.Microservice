using Knowit.Grpc.Correlation;
using Knowit.Grpc.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Serilog;

namespace Host
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			//Access settings / options like this:
			//var startupOptions = Configuration.GetSection("Startup").Get<StartupOptions>();

			services.AddCorrelationId();
			services.AddGrpcWeb();
			services.AddGrpc(options => { });

			services.AddDbContext<Database>(opt => opt.UseSqlServer(Configuration.GetConnectionString("Database")));
			services.Configure<DatabaseOptions>(options => Configuration.GetSection("Database").Bind(options));
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseCorrelationId();
			app.UseSerilogRequestLogging();
			app.UseDefaultFiles();
			app.UseStaticFiles();
			app.UseGrpcWeb();
			app.UseRouting();
			app.UseEndpoints(endpoints => endpoints.MapGrpcService<Service.Service>());
		}
	}
}
