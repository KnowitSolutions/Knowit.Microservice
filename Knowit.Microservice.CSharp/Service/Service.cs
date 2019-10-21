using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProjectName;
using Repository;

namespace Service
{
    public class Service : Core.CoreBase
    {
        private readonly ILogger<Service> _logger;
        private readonly Database _database;
        private readonly Other.OtherClient _otherClient;

        public Service(ILogger<Service> logger, Database database, Other.OtherClient otherClient)
        {
            _logger = logger;
            _database = database;
            _otherClient = otherClient;
        }

        public override Task<EchoResponse> Echo(EchoRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Information");
            return Task.FromResult(new EchoResponse {Message = request.Message});
        }
    }
}
