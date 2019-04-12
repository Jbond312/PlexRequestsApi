using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public interface IRequestService
    {
        Task<Request> GetRequestById(Guid id);
        Task<Request> GetExistingMovieRequest(AgentTypes agentType, string agentSourceId);
        Task<List<Request>> GetExistingTvRequests(AgentTypes agentType, string agentSourceId);
        Task Create(Request request);
        Task DeleteRequest(Guid id);
    }
}