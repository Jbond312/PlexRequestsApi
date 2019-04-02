using System;
using System.Collections.Generic;

namespace PlexRequests.Models
{
    public class PlexServerModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<PlexServerLibraryModel> Libraries { get; set; }
    }
}
