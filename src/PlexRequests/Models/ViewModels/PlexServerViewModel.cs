using System;
using System.Collections.Generic;

namespace PlexRequests.Models.ViewModels
{
    public class PlexServerViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<PlexServerLibraryViewModel> Libraries { get; set; }
    }
}
