using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PlexRequests.Api;
using PlexRequests.Settings;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.TheMovieDb
{
    public class TheMovieDbApi : ITheMovieDbApi
    {
        private const string Region = "GB";
        private const string LanguageCode = "en-GB";

        private readonly string _baseUri;
        private readonly string _apiKey;
        private readonly IApiService _apiService;

        public TheMovieDbApi(
            IApiService apiService, 
            IOptions<TheMovieDbSettings> settings)
        {
            _apiService = apiService;
            _baseUri = settings.Value.BaseUri;
            _apiKey = settings.Value.ApiKey;
        }

        public async Task<List<MovieSearch>> SearchMovies(string query, int? year, int? page)
        {
            var requestBuilder = new ApiRequestBuilder(_baseUri, "search/movie", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
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
            var apiRequest = new ApiRequestBuilder(_baseUri, "movie/upcoming", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
                .AddQueryParam("region", Region)
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<MovieSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<List<MovieSearch>> PopularMovies(int? page)
        {
            var apiRequest = new ApiRequestBuilder(_baseUri, "movie/popular", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
                .AddQueryParam("region", Region)
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<MovieSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<List<MovieSearch>> TopRatedMovies(int? page)
        {
            var apiRequest = new ApiRequestBuilder(_baseUri, "movie/top_rated", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
                .AddQueryParam("region", Region)
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<MovieSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<MovieDetails> GetMovieDetails(int movieId)
        {
            var apiRequest = new ApiRequestBuilder(_baseUri, $"movie/{movieId}", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
                .AddQueryParam("language", LanguageCode)
                .Build();

            var movie = await _apiService.InvokeApiAsync<MovieDetails>(apiRequest);

            return movie;
        }

        public async Task<ExternalIds> GetMovieExternalIds(int movieId)
        {
            var apiRequest = new ApiRequestBuilder(_baseUri, $"movie/{movieId}/external_ids", HttpMethod.Get)
                             .AddQueryParam("api_key", _apiKey)
                             .AddQueryParam("language", LanguageCode)
                             .Build();

            var externalIds = await _apiService.InvokeApiAsync<ExternalIds>(apiRequest);

            return externalIds;
        }

        public async Task<List<TvSearch>> SearchTv(string query, int? page)
        {
            var requestBuilder = new ApiRequestBuilder(_baseUri, "search/tv", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
                .AddQueryParam("query", query)
                .AddQueryParam("page", GetPage(page));

            var apiRequest = requestBuilder.Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<TvSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<List<TvSearch>> PopularTv(int? page)
        {
            var apiRequest = new ApiRequestBuilder(_baseUri, "tv/popular", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<TvSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<List<TvSearch>> TopRatedTv(int? page)
        {
            var apiRequest = new ApiRequestBuilder(_baseUri, "tv/top_rated", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
                .AddQueryParam("language", LanguageCode)
                .AddQueryParam("page", GetPage(page))
                .Build();

            var pagedResult = await _apiService.InvokeApiAsync<TheMovieDbPagedResult<TvSearch>>(apiRequest);

            return pagedResult.Results;
        }

        public async Task<TvDetails> GetTvDetails(int tvId)
        {
            var apiRequest = new ApiRequestBuilder(_baseUri, $"tv/{tvId}", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
                .AddQueryParam("language", LanguageCode)
                .Build();

            var tvDetails = await _apiService.InvokeApiAsync<TvDetails>(apiRequest);

            return tvDetails;
        }

        public async Task<TvSeasonDetails> GetTvSeasonDetails(int tvId, int seasonNumber)
        {
            var apiRequest = new ApiRequestBuilder(_baseUri, $"tv/{tvId}/season/{seasonNumber}", HttpMethod.Get)
                .AddQueryParam("api_key", _apiKey)
                .AddQueryParam("language", LanguageCode)
                .Build();

            var tvSeasonDetails = await _apiService.InvokeApiAsync<TvSeasonDetails>(apiRequest);

            return tvSeasonDetails;
        }

        public async Task<ExternalIds> GetTvExternalIds(int tvId)
        {
            var apiRequest = new ApiRequestBuilder(_baseUri, $"tv/{tvId}/external_ids", HttpMethod.Get)
                             .AddQueryParam("api_key", _apiKey)
                             .AddQueryParam("language", LanguageCode)
                             .Build();

            var externalIds = await _apiService.InvokeApiAsync<ExternalIds>(apiRequest);

            return externalIds;
        }

        private static string GetPage(int? page)
        {
            return page == null ? "1" : page.ToString();
        }
    }
}