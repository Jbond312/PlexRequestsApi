using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public interface IRequestRepository
    {
        Task Create(Request request);
        Task Update(Request request);
        Task<List<Request>> GetMany(Expression<Func<Request, bool>> filter = null);

        Task<Paged<Request>> GetPaged(string title, PlexMediaTypes? mediaType, RequestStatuses? status, Guid? userId, int? page,
            int? pageSize);
        Task<Request> GetOne(Expression<Func<Request, bool>> filter = null);
        Task Delete(Guid id);
    }
}