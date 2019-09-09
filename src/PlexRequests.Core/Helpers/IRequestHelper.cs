using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Helpers
{
    public interface IRequestHelper
    {
        void SetAggregatedStatus(TvRequest request);
    }
}