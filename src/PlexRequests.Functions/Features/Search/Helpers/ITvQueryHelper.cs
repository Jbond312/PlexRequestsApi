using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Functions.Features.Search.Models;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.Functions.Features.Search.Helpers
{
    public interface ITvQueryHelper
    {
        Task<List<TvSearchModel>> CreateSearchModels(List<TvSearch> queryResults);
        Task<TvDetailModel> CreateShowDetailModel(TvDetails tvDetails);
        Task<TvSeasonDetailModel> CreateSeasonDetailModel(int tvId, TvSeasonDetails tvSeasonDetails);
    }
}
