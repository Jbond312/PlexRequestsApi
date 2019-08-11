using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public class IssueService : IIssueService
    {
        private readonly IIssuesRepository _issueRepository;

        public IssueService(IIssuesRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        public async Task<Issue> GetIssueById(Guid id)
        {
            return await _issueRepository.GetOne(x => x.Id == id);
        }

        public async Task Create(Issue issue)
        {
            await _issueRepository.Create(issue);
        }

        public async Task Update(Issue issue)
        {
            await _issueRepository.Update(issue);
        }

        public async Task<List<Issue>> GetIncompleteIssues()
        {
            return await _issueRepository.GetMany(x => x.Status != IssueStatuses.Resolved);
        }

        public async Task<Paged<Issue>> GetPaged(int? page, int? pageSize, List<IssueStatuses> includeStatuses = null)
        {
            return await _issueRepository.GetPaged(page, includeStatuses == null ? null : pageSize, x => includeStatuses.Contains(x.Status));
        }
    }
}