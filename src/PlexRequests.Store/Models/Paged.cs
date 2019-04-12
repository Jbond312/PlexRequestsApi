using System.Collections.Generic;

namespace PlexRequests.Store.Models
{
    public class Paged<T> where T : class
    {
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<T> Items { get; set; }
    }
}