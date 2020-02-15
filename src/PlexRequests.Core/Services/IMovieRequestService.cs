using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Services
{
    public interface IMovieRequestService
    {
        Task<MovieRequestRow> GetRequestById(int id);
        Task<Paged<MovieRequestRow>> GetPaged(string title, RequestStatuses? status, int? userId, int? page,
            int? pageSize);
        Task<MovieRequestRow> GetExistingRequest(AgentTypes agentType, string agentSourceId);
        Task<List<MovieRequestRow>> GetIncompleteRequests();
        Task<Dictionary<int, MovieRequestRow>> GetRequestsByMovieDbIds(List<int> moviedbIds);
        Task Add(MovieRequestRow request);
        Task DeleteRequest(int id);
    }
}