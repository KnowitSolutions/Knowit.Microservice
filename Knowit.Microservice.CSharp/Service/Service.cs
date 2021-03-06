using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectName.Api;
using Repository;

namespace Service
{
	public class Service : Core.CoreBase
	{
		private readonly ILogger<Service> _logger;
		private readonly Database _database;
		private readonly ServiceOptions _options;
		private readonly Other.Api.Core.CoreClient _otherClient;

		public Service(ILogger<Service> logger, Database database, IOptions<ServiceOptions> options, Other.Api.Core.CoreClient otherClient)
		{
			_logger = logger;
			_database = database;
			_options = options.Value;
			_otherClient = otherClient;
		}

		public override Task<EchoResponse> Echo(EchoRequest request, ServerCallContext context)
		{
			_logger.LogInformation("Information");
			return Task.FromResult(new EchoResponse { Message = request.Message });
		}
	}
}
