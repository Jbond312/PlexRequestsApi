using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.TheMovieDb
{
    public interface ITheMovieDbApi
    {
        Task<List<MovieSearch>> SearchMovies(string query, int? year, int? page);
        Task<List<MovieSearch>> UpcomingMovies(int? page);
        Task<List<MovieSearch>> PopularMovies(int? page);
        Task<List<MovieSearch>> TopRatedMovies(int? page);
        Task<MovieDetails> GetMovieDetails(int movieId);
        Task<ExternalIds> GetMovieExternalIds(int movieId);
        Task<List<TvSearch>> SearchTv(string query, int? page);
        Task<List<TvSearch>> PopularTv(int? page);
        Task<List<TvSearch>> TopRatedTv(int? page);
        Task<TvDetails> GetTvDetails(int tvId);
        Task<TvSeasonDetails> GetTvSeasonDetails(int tvId, int seasonNumber);
    }
}
