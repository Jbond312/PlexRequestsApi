using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Core.Services;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.TheMovieDb
{
    public class TheMovieDbService : ITheMovieDbService
    {
        private readonly ICacheService _cacheService;
        private readonly ITheMovieDbApi _theMovieDbApi;

        public TheMovieDbService(
            ICacheService cacheService,
            ITheMovieDbApi theMovieDbApi
        )
        {
            _cacheService = cacheService;
            _theMovieDbApi = theMovieDbApi;
        }

        public async Task<MovieDetails> GetMovieDetails(int movieId)
        {
            var key = $"tmdb_GetMovieDetails_{movieId}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.GetMovieDetails(movieId));
        }

        public async Task<ExternalIds> GetMovieExternalIds(int movieId)
        {
            var key = $"tmdb_GetMovieExternalIds_{movieId}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.GetMovieExternalIds(movieId));
        }

        public async Task<TvDetails> GetTvDetails(int tvId)
        {
            var key = $"tmdb_GetTvDetails_{tvId}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.GetTvDetails(tvId));
        }

        public async Task<ExternalIds> GetTvExternalIds(int tvId)
        {
            var key = $"tmdb_GetTvExternalIds_{tvId}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.GetTvExternalIds(tvId));
        }

        public async Task<TvSeasonDetails> GetTvSeasonDetails(int tvId, int seasonNumber)
        {
            var key = $"tmdb_GetTvSeasonDetails_{tvId}|{seasonNumber}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.GetTvSeasonDetails(tvId, seasonNumber));
        }

        public async Task<List<MovieSearch>> PopularMovies(int? page)
        {
            var key = $"tmdb_PopularMovies_{page}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.PopularMovies(page));
        }

        public async Task<List<TvSearch>> PopularTv(int? page)
        {
            var key = $"tmdb_PopularTv_{page}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.PopularTv(page));
        }

        public async Task<List<MovieSearch>> SearchMovies(string query, int? year, int? page)
        {
            var key = $"tmdb_SearchMovies_{query}|{year}|{page}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.SearchMovies(query, year, page));
        }

        public async Task<List<TvSearch>> SearchTv(string query, int? page)
        {
            var key = $"tmdb_SearchTv_{query}|{page}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.SearchTv(query, page));
        }

        public async Task<List<MovieSearch>> TopRatedMovies(int? page)
        {
            var key = $"tmdb_TopRatedMovies_{page}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.TopRatedMovies(page));
        }

        public async Task<List<TvSearch>> TopRatedTv(int? page)
        {
            var key = $"tmdb_TopRatedTv_{page}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.TopRatedTv(page));
        }

        public async Task<List<MovieSearch>> UpcomingMovies(int? page)
        {
            var key = $"tmdb_UpcomingMovies_{page}";

            return await _cacheService.GetOrCreate(key, async () => await _theMovieDbApi.UpcomingMovies(page));
        }
    }
}