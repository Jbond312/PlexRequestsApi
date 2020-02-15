using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.DataAccess.Repositories;

namespace PlexRequests.Core.Services
{
    public class IssueService : IIssueService
    {
        private readonly IIssuesRepository _issueRepository;

        public IssueService(IIssuesRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        public async Task<IssueRow> GetIssueById(int id)
        {
            return await _issueRepository.GetOne(x => x.IssueId == id);
        }

        public void Add(IssueRow issue)
        {
            _issueRepository.Add(issue);
        }

        public async Task<List<IssueRow>> GetIncompleteIssues()
        {
            return await _issueRepository.GetMany(x => x.IssueStatus != IssueStatuses.Resolved);
        }

        public async Task<Paged<IssueRow>> GetPaged(int? page, int? pageSize, List<IssueStatuses> includeStatuses = null)
        {
            return await _issueRepository.GetPaged(page, includeStatuses == null ? null : pageSize, x => includeStatuses.Contains(x.IssueStatus));
        }
    }
}