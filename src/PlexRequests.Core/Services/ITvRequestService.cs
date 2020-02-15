using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Services
{
    public interface ITvRequestService
    {
        Task<TvRequestRow> GetRequestById(int id);
        Task<Paged<TvRequestRow>> GetPaged(string title, RequestStatuses? status, int? userId, int? page,
            int? pageSize);
        Task<TvRequestRow> GetExistingRequest(AgentTypes agentType, string agentSourceId);
        Task<List<TvRequestRow>> GetIncompleteRequests();
        Task<Dictionary<int, TvRequestRow>> GetRequestsByMovieDbIds(List<int> moviedbIds);
        Task Add(TvRequestRow request);
        Task DeleteRequest(int id);
        void SetAggregatedStatus(TvRequestRow request);
    }
}