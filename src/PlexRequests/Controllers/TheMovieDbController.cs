using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TheMovieDbController : Controller
    {
        private readonly ITheMovieDbApi _theMovieDbApi;

        public TheMovieDbController(ITheMovieDbApi theMovieDbApi)
        {
            _theMovieDbApi = theMovieDbApi;
        }

        [HttpGet("SearchMovies")]
        [SwaggerResponse(200, null, typeof(List<MovieSearch>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearch>>> SearchMovies([FromQuery][Required] string query, [FromQuery] int? year, [FromQuery] int? page)
        {
            return await _theMovieDbApi.SearchMovies(query, year, page);
        }

        [HttpGet("UpcomingMovies")]
        [SwaggerResponse(200, null, typeof(List<MovieSearch>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearch>>> UpcomingMovies([FromQuery] int? page)
        {
            return await _theMovieDbApi.UpcomingMovies(page);
        }

        [HttpGet("PopularMovies")]
        [SwaggerResponse(200, null, typeof(List<MovieSearch>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearch>>> PopularMovies([FromQuery] int? page)
        {
            return await _theMovieDbApi.PopularMovies(page);
        }

        [HttpGet("TopRatedMovies")]
        [SwaggerResponse(200, null, typeof(List<MovieSearch>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearch>>> TopRatedMovies([FromQuery] int? page)
        {
            return await _theMovieDbApi.TopRatedMovies(page);
        }

        [HttpGet("Movie/{movieId:int}")]
        [SwaggerResponse(200, null, typeof(MovieDetails))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<MovieDetails>> GetMovieDetails([FromRoute] int movieId)
        {
            return await _theMovieDbApi.GetMovieDetails(movieId);
        }

        [HttpGet("PopularTv")]
        [SwaggerResponse(200, null, typeof(List<TvSearch>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<TvSearch>>> PopularTv([FromQuery] int? page)
        {
            return await _theMovieDbApi.PopularTv(page);
        }

        [HttpGet("TopRatedTv")]
        [SwaggerResponse(200, null, typeof(List<TvSearch>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<TvSearch>>> TopRatedTv([FromQuery] int? page)
        {
            return await _theMovieDbApi.TopRatedTv(page);
        }

        [HttpGet("SearchTv")]
        [SwaggerResponse(200, null, typeof(List<TvSearch>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<TvSearch>>> SearchTv([FromQuery][Required] string query, [FromQuery] int? page)
        {
            return await _theMovieDbApi.SearchTv(query, page);
        }

        [HttpGet("TvDetails/{tvId:int}")]
        [SwaggerResponse(200, null, typeof(TvDetails))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<TvDetails>> TvDetails([FromRoute] int tvId)
        {
            return await _theMovieDbApi.GetTvDetails(tvId);
        }

        [HttpGet("TvDetails/{tvId:int}/Season/{seasonNumber:int}")]
        [SwaggerResponse(200, null, typeof(TvSeasonDetails))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<TvSeasonDetails>> TvSeasonDetails([FromRoute] int tvId, [FromRoute] int seasonNumber)
        {
            return await _theMovieDbApi.GetTvSeasonDetails(tvId, seasonNumber);
        }
    }
}
