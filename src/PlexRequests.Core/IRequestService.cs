using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public interface IRequestService
    {
        Task<List<Request>> GetMany(Expression<Func<Request, bool>> filter = null);
        Task<Request> GetOne(Expression<Func<Request, bool>> filter = null);
        Task Create(Request request);
    }
}