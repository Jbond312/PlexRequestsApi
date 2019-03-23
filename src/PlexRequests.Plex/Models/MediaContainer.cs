using System.Collections.Generic;

namespace PlexRequests.Plex.Models
{
    public class MediaContainer
    {
        //Sections
        public List<Directory> Directory { get; set; }

        //Sections / All
        public string Art { get; set; }
        public string Title2 { get; set; }
        public int LibrarySectionId { get; set; }
        public string LibrarySectionTitle { get; set; }
        public string LibrarySectionUuid { get; set; }
        public string Thumb { get; set; }
        public string ViewGroup { get; set; }
        public int ViewMode { get; set; }
        public bool NoCache { get; set; }
        public Metadata[] Metadata { get; set; }


        //Shared
        public int Size { get; set; }
        public bool AllowSync { get; set; }
        public string Identifier { get; set; }
        public string MediaTagPrefix { get; set; }
        public int MediaTagVersion { get; set; }
        public string Title1 { get; set; }
    }
}
