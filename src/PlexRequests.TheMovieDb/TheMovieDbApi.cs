using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PlexRequests.Api;
using PlexRequests.Settings;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.TheMovieDb
{
    public class TheMovieDbApi : ITheMovieDbApi
    {
        private const string BaseUri = "https://api.themoviedb.org/3/";
        private const string Region = "GB";
        private const string LanguageCode = "en-GB";

        private readonly IApiService _apiService;
        private readonly ISettingsService _settingsService;

        public TheMovieDbApi(IApiService apiService, ISettingsService settingsService)
        {
            _apiService = apiService;
            _settingsService = settingsService;
        }

        public async Task<List<MovieSearch>> SearchMovies(string query, int? year, int? page)
        {
            var requestBuilder = new ApiRequestBuilder(BaseUri, "search/movie", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("query", query)
                .AddQueryParam("page", GetPage(page));

            if (year.HasValue)
            {
                requestBuilder.AddQueryParam("year", year.Value.ToString());
            }

            var apiRequest = requestBuilder.Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<MovieSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<List<MovieSearch>> UpcomingMovies(int? page)
        {
            var apiRequest = new ApiRequestBuilder(BaseUri, "movie/upcoming", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("region", Region)
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<MovieSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<List<MovieSearch>> PopularMovies(int? page)
        {
            var apiRequest = new ApiRequestBuilder(BaseUri, "movie/popular", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("region", Region)
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<MovieSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<List<MovieSearch>> TopRatedMovies(int? page)
        {
            var apiRequest = new ApiRequestBuilder(BaseUri, "movie/top_rated", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("region", Region)
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<MovieSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<MovieDetails> GetMovieDetails(int movieId)
        {
            var apiRequest = new ApiRequestBuilder(BaseUri, $"movie/{movieId}", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("language", LanguageCode)
                .Build();

            var movie = await _apiService.InvokeApiAsync<MovieDetails>(apiRequest);

            return movie;
        }

        public async Task<List<TvSearch>> SearchTv(string query, int? page)
        {
            var requestBuilder = new ApiRequestBuilder(BaseUri, "search/tv", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("query", query)
                .AddQueryParam("page", GetPage(page));

            var apiRequest = requestBuilder.Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<TvSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<List<TvSearch>> PopularTv(int? page)
        {
            var apiRequest = new ApiRequestBuilder(BaseUri, "tv/popular", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<TvSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<List<TvSearch>> TopRatedTv(int? page)
        {
            var apiRequest = new ApiRequestBuilder(BaseUri, "tv/top_rated", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<TvSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<TvDetails> GetTvDetails(int tvId)
        {
            var apiRequest = new ApiRequestBuilder(BaseUri, $"tv/{tvId}", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("language", LanguageCode)
                .Build();

            var tvDetails = await _apiService.InvokeApiAsync<TvDetails>(apiRequest);

            return tvDetails;
        }

        public async Task<TvSeasonDetails> GetTvSeasonDetails(int tvId, int seasonNumber)
        {
            var apiRequest = new ApiRequestBuilder(BaseUri, $"tv/{tvId}/season/{seasonNumber}", HttpMethod.Get)
                .AddQueryParam("api_key", await GetApiKey())
                .AddQueryParam("language", LanguageCode)
                .Build();

            var tvSeasonDetails = await _apiService.InvokeApiAsync<TvSeasonDetails>(apiRequest);

            return tvSeasonDetails;
        }

        private async Task<string> GetApiKey()
        {
            var settings = await _settingsService.Get();

            return settings.TheMovieDbApiKey;
        }

        private static string GetPage(int? page)
        {
            return page == null ? "1" : page.ToString();
        }
    }
}