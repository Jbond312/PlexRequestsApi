using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PlexRequests.Plex.Models.OAuth;

namespace PlexRequests.Plex
{
    public class PlexApi : IPlexApi
    {
        private string _baseUri = "https://plex.tv/api/v2/";

        public async Task<OAuthPin> CreatePin()
        {
            throw new NotImplementedException();
        }
    }
}
