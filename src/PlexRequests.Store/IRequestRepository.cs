using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public interface IRequestRepository
    {
        Task Create(Request request);
        Task<List<Request>> GetMany(Expression<Func<Request, bool>> filter = null);
        Task<Request> GetOne(Expression<Func<Request, bool>> filter = null);
    }
}