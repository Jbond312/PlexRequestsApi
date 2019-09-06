using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Helpers
{
    public interface IRequestHelper
    {
        RequestStatuses CalculateAggregatedStatus(TvRequest request);
    }
}