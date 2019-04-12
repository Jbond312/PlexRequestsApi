using System.Collections.Generic;
using PlexRequests.Models.ViewModels;

namespace PlexRequests.Models.Requests
{
    public class GetPagedRequestQueryResult
    {
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<RequestViewModel> Items { get; set; }
    }
}