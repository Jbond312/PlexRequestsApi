using System.Collections.Generic;
using PlexRequests.Models.ViewModels;

namespace PlexRequests.Models.Plex
{
    public class SyncLibrariesCommandResult
    {
        public List<PlexServerLibraryViewModel> Libraries { get; set; }
    }
}
