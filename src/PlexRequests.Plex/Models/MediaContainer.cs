using System.Collections.Generic;

namespace PlexRequests.Plex.Models
{
    public class MediaContainer
    {
        //Movie
        public int Size { get; set; }
        public bool AllowSync { get; set; }
        public string Identifier { get; set; }
        public int LibrarySectionID { get; set; }
        public string LibrarySectionTitle { get; set; }
        public string LibrarySectionUUID { get; set; }
        public string MediaTagPrefix { get; set; }
        public int MediaTagVersion { get; set; }
        public List<Metadata> Metadata { get; set; }

        //Library Sections
        public string Art { get; set; }
        public bool NoCache { get; set; }
        public string Thumb { get; set; }
        public string Title1 { get; set; }
        public string Title2 { get; set; }
        public string ViewGroup { get; set; }
        public int ViewMode { get; set; }


        //TV Show Seasons
        public string Banner { get; set; }
        public string Key { get; set; }
        public int ParentIndex { get; set; }
        public string ParentTitle { get; set; }
        public int ParentYear { get; set; }
        public bool SortAsc { get; set; }
        public string Summary { get; set; }
        public string Theme { get; set; }
        public List<Directory> Directory { get; set; }

        //TV Show Episode
        public string GrandParentContentRating { get; set; }
        public int GrandParentRatingKey { get; set; }
        public string GrandParentStudio { get; set; }
        public string GrandParentTheme { get; set; }
        public string GrandParentThumb { get; set; }
        public string GrandParentTitle { get; set; }
    }
}
