using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using PlexRequests.Functions.Features.Health.Models;

namespace PlexRequests.Functions.Features.Health.Queries
{
    public class GetHealthQueryHandler : IRequestHandler<GetHealthQuery, GetHealthQueryResult>
    {
        private readonly IConfiguration _configuration;

        public GetHealthQueryHandler(
            IConfiguration configuration
            )
        {
            _configuration = configuration;
        }

        public async Task<GetHealthQueryResult> Handle(GetHealthQuery request, CancellationToken cancellationToken)
        {
            var result = new GetHealthQueryResult
            {
                Data = new HealthModel
                {
                    Version = _configuration["PlexRequestsSettings:Version"],
                    ApplicationName = _configuration["PlexRequestsSettings:ApplicationName"],
                    TimeStamp = DateTime.UtcNow
                }
            };

            return await Task.FromResult(result);
        }
    }
}
