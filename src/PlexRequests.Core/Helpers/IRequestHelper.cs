using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Core.Helpers
{
    public interface IRequestHelper
    {
        void SetAggregatedStatus(TvRequestRow request);
    }
}