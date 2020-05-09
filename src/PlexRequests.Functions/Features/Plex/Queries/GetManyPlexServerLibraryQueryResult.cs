using System.Collections.Generic;
using PlexRequests.Functions.Features.Plex.Models.Detail;

namespace PlexRequests.Functions.Features.Plex.Queries
{
    public class GetManyPlexServerLibraryQueryResult
    {
        public List<PlexServerLibraryDetailModel> Libraries { get; set; }
    }
}