using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Services
{
    public interface IIssueService
    {
        Task<IssueRow> GetIssueById(int id);
        Task<List<IssueRow>> GetIncompleteIssues();
        Task<Paged<IssueRow>> GetPaged(int? page, int? pageSize, List<IssueStatuses> includeStatuses = null);
        void Add(IssueRow issue);

    }
}