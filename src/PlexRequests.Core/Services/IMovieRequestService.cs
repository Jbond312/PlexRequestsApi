using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public interface IMovieRequestService
    {
        Task<MovieRequest> GetRequestById(Guid id);
        Task<Paged<MovieRequest>> GetPaged(string title, RequestStatuses? status, Guid? userId, int? page,
            int? pageSize);
        Task<MovieRequest> GetExistingRequest(AgentTypes agentType, string agentSourceId);
        Task<List<MovieRequest>> GetIncompleteRequests();
        Task Create(MovieRequest request);
        Task Update(MovieRequest request);
        Task DeleteRequest(Guid id);
    }
}