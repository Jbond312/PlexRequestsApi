using System;
using System.Collections.Generic;

namespace PlexRequests.ApiRequests.Plex.DTOs.Detail
{
    public class PlexServerDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<PlexServerLibraryDetailModel> Libraries { get; set; }
    }
}
