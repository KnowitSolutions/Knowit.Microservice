using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Knowit.Grpc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectName;
using NUnit.Framework;
using Repository;

namespace Tests
{
    public class ServiceTests : ServiceTests<Core.CoreClient, Service.Service>
    {
        
        [NotNull]
        private Database? _database;

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddDbContext<Database>(options =>
                options.UseInMemoryDatabase("TestDatabase")
            );
        }

        [SetUp]
        public void Setup()
        {
            _database = Services.GetRequiredService<Database>();
            // entities will persist between tests, remove them if this is undesirable
            _database.Entity.RemoveRange(_database.Entity);
        }

        [Test]
        public async Task TestEcho()
        {
            var request = new EchoRequest {Message = "Hello World"};
            var response = await Client.EchoAsync(request);
            Assert.AreEqual(request.Message, response.Message);
        }
    }
}
