using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public interface IRequestRepository
    {
        Task Create(Request request);
        Task<List<Request>> GetMany(Expression<Func<Request, bool>> filter = null);

        Task<Paged<Request>> GetPaged(string title, PlexMediaTypes? mediaType, bool? isApproved, Guid? userId, int? page,
            int? pageSize);
        Task<Request> GetOne(Expression<Func<Request, bool>> filter = null);
        Task Delete(Guid id);
    }
}