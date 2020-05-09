using System.Collections.Generic;

namespace PlexRequests.Functions.Features.Plex.Models.Detail
{
    public class PlexServerDetailModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<PlexServerLibraryDetailModel> Libraries { get; set; }
    }
}
