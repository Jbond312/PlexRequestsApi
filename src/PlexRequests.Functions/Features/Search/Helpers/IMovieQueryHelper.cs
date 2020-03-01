using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.Functions.Features.Search.Helpers
{
    public interface IMovieQueryHelper
    {
        Task<List<MovieSearchModel>> CreateSearchModels(List<MovieSearch> queryResults);
        Task<MovieDetailModel> CreateDetailModel(MovieDetails movieDetails);
    }
}
