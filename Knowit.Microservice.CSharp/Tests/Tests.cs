using System.Threading.Tasks;
using Knowit.Grpc.Testing;
using ProjectName;
using NUnit.Framework;

namespace Tests
{
    public class ServiceTests : ServiceTests<Core.CoreClient, Service.Service>
    {
        [Test]
        public async Task TestEcho()
        {
            var request = new EchoRequest {Message = "Hello World"};
            var response = await Client.EchoAsync(request);
            Assert.AreEqual(request.Message, response.Message);
        }
    }
}