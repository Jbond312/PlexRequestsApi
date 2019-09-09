using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public interface ITvRequestService
    {
        Task<TvRequest> GetRequestById(Guid id);
        Task<Paged<TvRequest>> GetPaged(string title, RequestStatuses? status, Guid? userId, int? page,
            int? pageSize);
        Task<List<TvRequest>> GetExistingRequests(AgentTypes agentType, string agentSourceId);
        Task<List<TvRequest>> GetIncompleteRequests();
        Task<Dictionary<int, TvRequest>> GetRequestsByMovieDbIds(List<int> moviedbIds);
        Task Create(TvRequest request);
        Task Update(TvRequest request);
        Task DeleteRequest(Guid id);
        void SetAggregatedStatus(TvRequest request);
    }
}