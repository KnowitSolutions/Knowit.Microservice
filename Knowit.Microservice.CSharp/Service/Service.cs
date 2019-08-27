using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProjectName;

namespace Service
{
    public class Service : Core.CoreBase
    {
        private readonly ILogger<Service> _logger;

        public Service(ILogger<Service> logger)
        {
            _logger = logger;
        }

        public override Task<EchoResponse> Echo(EchoRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Information");
            return Task.FromResult(new EchoResponse {Message = request.Message});
        }
    }
}