using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.ApiRequests.Search.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController : Controller
    {
        private readonly IMediator _mediator;

        public SearchController(
            IMediator mediator
            )
        {
            _mediator = mediator;
        }

        [HttpGet("Movies")]
        [SwaggerResponse(200, null, typeof(List<MovieSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearchModel>>> SearchMovies([FromQuery][Required] string query, [FromQuery] int? year, [FromQuery] int? page)
        {
            var searchQuery = new SearchMovieQuery(query, year, page);
            return await _mediator.Send(searchQuery);
        }

        [HttpGet("Movies/Upcoming")]
        [SwaggerResponse(200, null, typeof(List<MovieSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearchModel>>> MoviesUpcoming([FromQuery] int? page)
        {
            var searchQuery = new UpcomingMovieQuery(page);
            return await _mediator.Send(searchQuery);
        }

        [HttpGet("Movies/Popular")]
        [SwaggerResponse(200, null, typeof(List<MovieSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearchModel>>> MoviesPopular([FromQuery] int? page)
        {
            var searchQuery = new PopularMovieQuery(page);
            return await _mediator.Send(searchQuery);
        }

        [HttpGet("Movies/TopRated")]
        [SwaggerResponse(200, null, typeof(List<MovieSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearchModel>>> MoviesTopRated([FromQuery] int? page)
        {
            var searchQuery = new TopRatedMovieQuery(page);
            return await _mediator.Send(searchQuery);
        }

        [HttpGet("Movies/{movieId:int}")]
        [SwaggerResponse(200, null, typeof(MovieDetailModel))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<MovieDetailModel>> GetMovieDetails([FromRoute] int movieId)
        {
            var query = new GetMovieDetailsQuery(movieId);
            return await _mediator.Send(query);
        }

        [HttpGet("Tv")]
        [SwaggerResponse(200, null, typeof(List<TvSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<TvSearchModel>>> SearchTv([FromQuery][Required] string query, [FromQuery] int? page)
        {
            var searchQuery = new SearchTvQuery(query, page);
            return await _mediator.Send(searchQuery);
        }

        [HttpGet("Tv/Popular")]
        [SwaggerResponse(200, null, typeof(List<TvSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<TvSearchModel>>> TvPopular([FromQuery] int? page)
        {
            var searchQuery = new PopularTvQuery(page);
            return await _mediator.Send(searchQuery);
        }

        [HttpGet("Tv/TopRated")]
        [SwaggerResponse(200, null, typeof(List<TvSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<TvSearchModel>>> TvTopRated([FromQuery] int? page)
        {
            var searchQuery = new TopRatedTvQuery(page);
            return await _mediator.Send(searchQuery);
        }

        [HttpGet("Tv/{tvId:int}")]
        [SwaggerResponse(200, null, typeof(TvDetailModel))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<TvDetailModel>> GetTvDetails([FromRoute] int tvId)
        {
            var query = new GetTvDetailsQuery(tvId);
            return await _mediator.Send(query);
        }

        [HttpGet("Tv/{tvId:int}/Season/{seasonNumber:int}")]
        [SwaggerResponse(200, null, typeof(TvSeasonDetailModel))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<TvSeasonDetailModel>> GetTvSeasonDetails([FromRoute] int tvId, [FromRoute] int seasonNumber)
        {
            var query = new GetTvSeasonDetailsQuery(tvId, seasonNumber);
            return await _mediator.Send(query);
        }
    }
}
