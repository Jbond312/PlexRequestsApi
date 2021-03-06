﻿using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PlexRequests.Functions.Features.Auth.Commands;

namespace PlexRequests.Functions.HttpTriggers
{
    public class ExpiredTokensTimerTrigger
    {
        private readonly IMediator _mediator;

        public ExpiredTokensTimerTrigger(
            IMediator mediator
            )
        {
            _mediator = mediator;
        }

        [FunctionName("RemoveExpiredTokens")]
        // ReSharper disable once UnusedParameter.Global
        public Task RemoveExpiredTokens([TimerTrigger("0 0 0 * * *")]TimerInfo timer, ILogger logger)
        {
            logger.LogTrace("Removing all expired refresh tokens");

            //Runs every day at 12:00 AM
            var command = new RemoveExpiredRefreshTokensCommand();

            return _mediator.Send(command);
        }
    }
}
