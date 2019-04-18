using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public interface IRequestService
    {
        Task<Request> GetRequestById(Guid id);
        Task<Paged<Request>> GetPaged(string title, PlexMediaTypes? mediaType, RequestStatuses? status, Guid? userId, int? page,
            int? pageSize);
        Task<Request> GetExistingMovieRequest(AgentTypes agentType, string agentSourceId);
        Task<List<Request>> GetExistingTvRequests(AgentTypes agentType, string agentSourceId);
        Task<List<Request>> GetIncompleteRequests(PlexMediaTypes mediaType);
        Task Create(Request request);
        Task Update(Request request);
        Task DeleteRequest(Guid id);
        RequestStatuses CalculateAggregatedStatus(Request request);
    }
}