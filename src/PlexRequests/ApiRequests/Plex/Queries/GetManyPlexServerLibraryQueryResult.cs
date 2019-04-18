using System.Collections.Generic;
using PlexRequests.ApiRequests.Plex.DTOs.Detail;

namespace PlexRequests.ApiRequests.Plex.Queries
{
    public class GetManyPlexServerLibraryQueryResult
    {
        public List<PlexServerLibraryDetailModel> Libraries { get; set; }
    }
}