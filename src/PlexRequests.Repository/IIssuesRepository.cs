using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public interface IIssuesRepository
    {
        Task Create(Issue issue);
        Task Update(Issue issue);
        Task<List<Issue>> GetMany(Expression<Func<Issue, bool>> filter = null);
        Task<Paged<Issue>> GetPaged(int? page, int? pageSize, Expression<Func<Issue, bool>> filter = null);
        Task<Issue> GetOne(Expression<Func<Issue, bool>> filter = null);
    }
}