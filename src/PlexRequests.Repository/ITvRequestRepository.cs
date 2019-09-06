using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public interface ITvRequestRepository
    {
        Task Create(TvRequest request);
        Task Update(TvRequest request);
        Task<List<TvRequest>> GetMany(Expression<Func<TvRequest, bool>> filter = null);

        Task<Paged<TvRequest>> GetPaged(string title, RequestStatuses? status, Guid? userId, int? page, int? pageSize);
        Task<TvRequest> GetOne(Expression<Func<TvRequest, bool>> filter = null);
        Task Delete(Guid id);
    }
}