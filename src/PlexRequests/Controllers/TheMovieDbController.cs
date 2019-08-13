using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.ApiRequests.TheMovieDb.DTOs;
using PlexRequests.TheMovieDb;
using Swashbuckle.AspNetCore.Annotations;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TheMovieDbController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITheMovieDbApi _theMovieDbApi;

        public TheMovieDbController(
            IMapper mapper,
            ITheMovieDbApi theMovieDbApi)
        {
            _mapper = mapper;
            _theMovieDbApi = theMovieDbApi;
        }

        [HttpGet("SearchMovies")]
        [SwaggerResponse(200, null, typeof(List<MovieSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearchModel>>> SearchMovies([FromQuery][Required] string query, [FromQuery] int? year, [FromQuery] int? page)
        {
            var movies = await _theMovieDbApi.SearchMovies(query, year, page);
            return _mapper.Map<List<MovieSearchModel>>(movies);
        }

        [HttpGet("UpcomingMovies")]
        [SwaggerResponse(200, null, typeof(List<MovieSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearchModel>>> UpcomingMovies([FromQuery] int? page)
        {
            var movies = await _theMovieDbApi.UpcomingMovies(page);
            return _mapper.Map<List<MovieSearchModel>>(movies);
        }

        [HttpGet("PopularMovies")]
        [SwaggerResponse(200, null, typeof(List<MovieSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearchModel>>> PopularMovies([FromQuery] int? page)
        {
            var movies = await _theMovieDbApi.PopularMovies(page);
            return _mapper.Map<List<MovieSearchModel>>(movies);
        }

        [HttpGet("TopRatedMovies")]
        [SwaggerResponse(200, null, typeof(List<MovieSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<MovieSearchModel>>> TopRatedMovies([FromQuery] int? page)
        {
            var movies = await _theMovieDbApi.TopRatedMovies(page);
            return _mapper.Map<List<MovieSearchModel>>(movies);
        }

        [HttpGet("Movie/{movieId:int}")]
        [SwaggerResponse(200, null, typeof(MovieDetailModel))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<MovieDetailModel>> GetMovieDetails([FromRoute] int movieId)
        {
            var movie = await _theMovieDbApi.GetMovieDetails(movieId);
            return _mapper.Map<MovieDetailModel>(movie);
        }

        [HttpGet("PopularTv")]
        [SwaggerResponse(200, null, typeof(List<TvSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<TvSearchModel>>> PopularTv([FromQuery] int? page)
        {
            var tvShows = await _theMovieDbApi.PopularTv(page);
            return _mapper.Map<List<TvSearchModel>>(tvShows);
        }

        [HttpGet("TopRatedTv")]
        [SwaggerResponse(200, null, typeof(List<TvSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<TvSearchModel>>> TopRatedTv([FromQuery] int? page)
        {
            var tvShows = await _theMovieDbApi.TopRatedTv(page);
            return _mapper.Map<List<TvSearchModel>>(tvShows);
        }

        [HttpGet("SearchTv")]
        [SwaggerResponse(200, null, typeof(List<TvSearchModel>))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<List<TvSearchModel>>> SearchTv([FromQuery][Required] string query, [FromQuery] int? page)
        {
            var tvShows = await _theMovieDbApi.SearchTv(query, page);
            return _mapper.Map<List<TvSearchModel>>(tvShows);
        }

        [HttpGet("TvDetails/{tvId:int}")]
        [SwaggerResponse(200, null, typeof(TvDetailModel))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<TvDetailModel>> TvDetails([FromRoute] int tvId)
        {
            var tvShow = await _theMovieDbApi.GetTvDetails(tvId);
            return _mapper.Map<TvDetailModel>(tvShow);
        }

        [HttpGet("TvDetails/{tvId:int}/Season/{seasonNumber:int}")]
        [SwaggerResponse(200, null, typeof(TvSeasonDetailModel))]
        [SwaggerResponse(401)]
        public async Task<ActionResult<TvSeasonDetailModel>> TvSeasonDetails([FromRoute] int tvId, [FromRoute] int seasonNumber)
        {
            var tvSeason = await _theMovieDbApi.GetTvSeasonDetails(tvId, seasonNumber);
            return _mapper.Map<TvSeasonDetailModel>(tvSeason);
        }
    }
}
