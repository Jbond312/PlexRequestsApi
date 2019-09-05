using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public interface IMovieRequestRepository
    {
        Task Create(MovieRequest request);
        Task Update(MovieRequest request);
        Task<List<MovieRequest>> GetMany(Expression<Func<MovieRequest, bool>> filter = null);

        Task<Paged<MovieRequest>> GetPaged(string title, RequestStatuses? status, Guid? userId, int? page,
            int? pageSize);
        Task<MovieRequest> GetOne(Expression<Func<MovieRequest, bool>> filter = null);
        Task Delete(Guid id);
    }
}