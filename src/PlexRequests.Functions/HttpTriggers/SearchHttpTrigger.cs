using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlexRequests.Functions.AccessTokens;
using PlexRequests.Functions.Features.Search.Queries;

namespace PlexRequests.Functions.HttpTriggers
{
    public class SearchHttpTrigger
    {
        private readonly IMediator _mediator;
        private readonly IAccessTokenProvider _accessTokenProvider;

        public SearchHttpTrigger(
            IMediator mediator,
            IAccessTokenProvider accessTokenProvider
        )
        {
            _mediator = mediator;
            _accessTokenProvider = accessTokenProvider;
        }

        [FunctionName("SearchMovies")]
        public async Task<IActionResult> SearchMovies(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/movies")]
            HttpRequest req,
            ILogger log)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (accessResult.Status != AccessTokenStatus.Valid)
            {
                return new UnauthorizedResult();
            }

            string query = req.Query["query"];
            int? year = null;
            int? page = null;

            if (int.TryParse(req.Query["year"], out var requestedYear))
            {
                year = requestedYear;
            }

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            var searchQuery = new SearchMovieQuery(query, year, page);

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }
    }
}
