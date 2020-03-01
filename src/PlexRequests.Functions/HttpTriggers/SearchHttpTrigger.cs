using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlexRequests.Functions.AccessTokens;
using PlexRequests.Functions.Features.Search.Queries;

namespace PlexRequests.Functions.HttpTriggers
{
    public class SearchHttpTrigger
    {
        private readonly IMediator _mediator;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly IRequestValidator _requestValidator;

        public SearchHttpTrigger(
            IMediator mediator,
            IAccessTokenProvider accessTokenProvider,
            IRequestValidator requestValidator
        )
        {
            _mediator = mediator;
            _accessTokenProvider = accessTokenProvider;
            _requestValidator = requestValidator;
        }

        [FunctionName("SearchMovies")]
        public async Task<IActionResult> SearchMovies(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/movies")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
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

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<SearchMovieQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }

        [FunctionName("SearchMoviesUpcoming")]
        public async Task<IActionResult> SearchMoviesUpcoming(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/movies/upcoming")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            int? page = null;

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            var searchQuery = new UpcomingMovieQuery(page);

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<UpcomingMovieQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }

        [FunctionName("SearchMoviesPopular")]
        public async Task<IActionResult> SearchMoviesPopular(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/movies/popular")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            int? page = null;

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            var searchQuery = new PopularMovieQuery(page);

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<PopularMovieQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }

        [FunctionName("SearchMoviesTopRated")]
        public async Task<IActionResult> SearchMoviesTopRated(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/movies/toprated")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            int? page = null;

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            var searchQuery = new TopRatedMovieQuery(page);

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<TopRatedMovieQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }

        [FunctionName("GetMovieDetails")]
        public async Task<IActionResult> GetMovieDetails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/movies/{movieId:int}")]
            HttpRequest req, int movieId)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }
            
            var searchQuery = new GetMovieDetailsQuery(movieId);

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<GetMovieDetailsQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }

        [FunctionName("SearchTv")]
        public async Task<IActionResult> SearhTv(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/tv")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            string query = req.Query["query"];
            int? page = null;

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            var searchQuery = new SearchTvQuery(query, page);

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<SearchTvQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }

        [FunctionName("SearchTvPopular")]
        public async Task<IActionResult> SearchTvPopular(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/tv/popular")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            int? page = null;

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            var searchQuery = new PopularTvQuery(page);

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<PopularTvQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }

        [FunctionName("SearchTvTopRated")]
        public async Task<IActionResult> SearchTvTopRated(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/tv/toprated")]
            HttpRequest req)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            int? page = null;

            if (int.TryParse(req.Query["page"], out var requestedPage))
            {
                page = requestedPage;
            }

            var searchQuery = new TopRatedTvQuery(page);

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<TopRatedTvQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }

        [FunctionName("GetTvDetails")]
        public async Task<IActionResult> GetTvDetails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/tv/{tvId:int}")]
            HttpRequest req, int tvId)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var searchQuery = new GetTvDetailsQuery(tvId);

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<GetTvDetailsQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }

        [FunctionName("GetTvSeasonDetails")]
        public async Task<IActionResult> GetTvSeasonDetails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/tv/{tvId:int}/season/{seasonNumber:int}")]
            HttpRequest req, int tvId, int seasonNumber)
        {
            var accessResult = _accessTokenProvider.ValidateToken(req);

            if (!accessResult.IsValid)
            {
                return new UnauthorizedResult();
            }

            var searchQuery = new GetTvSeasonDetailsQuery(tvId, seasonNumber);

            var queryValidationResult = _requestValidator.ValidateRequest(searchQuery);

            if (!queryValidationResult.IsSuccessful)
            {
                return queryValidationResult.ToResultIfValid<GetTvSeasonDetailsQuery, BadRequestResult>();
            }

            var response = await _mediator.Send(searchQuery);

            return new OkObjectResult(response);
        }
    }
}
