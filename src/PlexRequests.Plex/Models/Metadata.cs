using System.Collections.Generic;

namespace PlexRequests.Plex.Models
{
    public class Metadata
    {
        //Movie
        public string RatingKey { get; set; }
        public string Key { get; set; }
        public string Guid { get; set; }
        public string LibrarySectionTitle { get; set; }
        public int LibrarySectionId { get; set; }
        public string LibrarySectionKey { get; set; }
        public string Studio { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string ContentRating { get; set; }
        public string Summary { get; set; }
        public double Rating { get; set; }
        public int ViewCount { get; set; }
        public int LastViewedAt { get; set; }
        public int Year { get; set; }
        public string TagLine { get; set; }
        public string Thumb { get; set; }
        public string Art { get; set; }
        public int Duration { get; set; }
        public string OriginallyAvailableAt { get; set; }
        public int AddedAt { get; set; }
        public int UpdatedAt { get; set; }
        public string ChapterSource { get; set; }
        public string RatingImage { get; set; }
        public List<Medium> Media { get; set; }
        public List<Genre> Genre { get; set; }
        public List<Director> Director { get; set; }
        public List<Writer> Writer { get; set; }
        public List<Producer> Producer { get; set; }
        public List<Country> Country { get; set; }
        public List<MediaRole> Role { get; set; }
        public List<Similar> Similar { get; set; }
        public List<Field> Field { get; set; }



        //Library Sections
        public string TitleSort { get; set; }
        public int Index { get; set; }
        public string Banner { get; set; }
        public int LeafCount { get; set; }
        public int ViewedLeafCount { get; set; }

        public int ChildCount { get; set; }
        public string Theme { get; set; }

        //TV Show Seasons
        public string ParentRatingKey { get; set; }
        public string ParentKey { get; set; }
        public string ParentTitle { get; set; }
        public int ParentIndex { get; set; }
        public string ParentThumb { get; set; }
        public string ParentTheme { get; set; }

        //TV Show Episode
        public string GrandParentRatingKey { get; set; }
        public string GrandParentKey { get; set; }
        public string GrandParentTitle { get; set; }
        public string GrandParentThumb { get; set; }
        public string GrandParentArt { get; set; }
        public string GrandParentTheme { get; set; }

        //Movie Section
        public string PrimaryExtraKey { get; set; }
        public List<Collection> Collection { get; set; }
        public string OriginalTitle { get; set; }
        public int? ViewOffset { get; set; }
    }
}
