using System;
using System.Collections.Generic;

namespace PlexRequests.Models.SubModels.Detail
{
    public class PlexServerDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<PlexServerLibraryDetailModel> Libraries { get; set; }
    }
}
