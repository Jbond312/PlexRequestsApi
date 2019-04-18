using System.Collections.Generic;
using PlexRequests.Models.SubModels.Detail;

namespace PlexRequests.Models.Plex
{
    public class GetManyPlexServerLibraryQueryResult
    {
        public List<PlexServerLibraryDetailModel> Libraries { get; set; }
    }
}