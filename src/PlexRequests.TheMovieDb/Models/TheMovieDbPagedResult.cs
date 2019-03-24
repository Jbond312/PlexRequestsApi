using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace PlexRequests.TheMovieDb.Models
{
    public class TheMovieDbPagedResult<T> where T : class
    {
        public int Page { get; set; }
        public int Total_Results { get; set; }
        public int Total_Pages { get; set; }
        public List<T> Results { get; set; }
    }
}
