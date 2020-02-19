﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using PlexRequests.ApiRequests.Health.Models;

namespace PlexRequests.ApiRequests.Health.Queries
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
            return new GetHealthQueryResult
            {
                Data = new HealthModel
                {
                    Version = _configuration["PlexRequestsSettings:Version"],
                    ApplicationName = _configuration["PlexRequestsSettings:ApplicationName"],
                    TimeStamp = DateTime.UtcNow
                }
            };
        }
    }
}