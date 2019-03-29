using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.TheMovieDb;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TheMovieDbController : Controller
    {
        private readonly ITheMovieDbApi _theMovieDbApi;

        public TheMovieDbController(ITheMovieDbApi theMovieDbApi)
        {
            _theMovieDbApi = theMovieDbApi;
        }

        [HttpGet("SearchMovies")]
        public async Task<ActionResult<List<MovieSearch>>> SearchMovies([FromQuery][Required] string query, [FromQuery] int? year, [FromQuery] int? page)
        {
            return await _theMovieDbApi.SearchMovies(query, year, page);
        }

        [HttpGet("UpcomingMovies")]
        public async Task<ActionResult<List<MovieSearch>>> UpcomingMovies([FromQuery] int? page)
        {
            return await _theMovieDbApi.UpcomingMovies(page);
        }

        [HttpGet("PopularMovies")]
        public async Task<ActionResult<List<MovieSearch>>> PopularMovies([FromQuery] int? page)
        {
            return await _theMovieDbApi.PopularMovies(page);
        }

        [HttpGet("TopRatedMovies")]
        public async Task<ActionResult<List<MovieSearch>>> TopRatedMovies([FromQuery] int? page)
        {
            return await _theMovieDbApi.TopRatedMovies(page);
        }

        [HttpGet("Movie/{movieId:int}")]
        public async Task<ActionResult<MovieDetails>> GetMovieDetails([FromRoute] int movieId)
        {
            return await _theMovieDbApi.GetMovieDetails(movieId);
        }

        [HttpGet("PopularTv")]
        public async Task<ActionResult<List<TvSearch>>> PopularTv([FromQuery] int? page)
        {
            return await _theMovieDbApi.PopularTv(page);
        }

        [HttpGet("TopRatedTv")]
        public async Task<ActionResult<List<TvSearch>>> TopRatedTv([FromQuery] int? page)
        {
            return await _theMovieDbApi.TopRatedTv(page);
        }

        [HttpGet("SearchTv")]
        public async Task<ActionResult<List<TvSearch>>> SearchTv([FromQuery][Required] string query, [FromQuery] int? page)
        {
            return await _theMovieDbApi.SearchTv(query, page);
        }

        [HttpGet("TvDetails/{tvId:int}")]
        public async Task<ActionResult<TvDetails>> TvDetails([FromRoute] int tvId)
        {
            return await _theMovieDbApi.GetTvDetails(tvId);
        }

        [HttpGet("TvDetails/{tvId:int}/Season/{seasonNumber:int}")]
        public async Task<ActionResult<TvSeasonDetails>> TvSeasonDetails([FromRoute] int tvId, [FromRoute] int seasonNumber)
        {
            return await _theMovieDbApi.GetTvSeasonDetails(tvId, seasonNumber);
        }
    }
}
