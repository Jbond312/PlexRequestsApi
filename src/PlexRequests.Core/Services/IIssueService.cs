using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public interface IIssueService
    {
        Task<Issue> GetIssueById(Guid id);
        Task<List<Issue>> GetIncompleteIssues();
        Task<Paged<Issue>> GetPaged(int? page, int? pageSize, List<IssueStatuses> includeStatuses = null);
        Task Create(Issue issue);
        Task Update(Issue issue);

    }
}