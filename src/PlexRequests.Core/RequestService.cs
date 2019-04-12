using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;

        public RequestService(
            IRequestRepository requestRepository
            )
        {
            _requestRepository = requestRepository;
        }

        public async Task<Request> GetRequestById(Guid id)
        {
            return await _requestRepository.GetOne(x => x.Id == id);
        }

        public async Task<Paged<Request>> GetPaged(string title, PlexMediaTypes? mediaType, bool? isApproved, Guid? userId, int? page, int? pageSize)
        {
            return await _requestRepository.GetPaged(title, mediaType, isApproved, userId, page, pageSize);
        }

        public async Task<Request> GetExistingMovieRequest(AgentTypes agentType, string agentSourceId)
        {
            return await _requestRepository.GetOne(x =>
                x.MediaType == PlexMediaTypes.Movie && x.AgentType == agentType && x.AgentSourceId == agentSourceId);
        }

        public async Task<List<Request>> GetExistingTvRequests(AgentTypes agentType, string agentSourceId)
        {
            return await _requestRepository.GetMany(x =>
                x.MediaType == PlexMediaTypes.Show && x.AgentType == agentType && x.AgentSourceId == agentSourceId);
        }

        public async Task Create(Request request)
        {
            await _requestRepository.Create(request);
        }

        public async Task DeleteRequest(Guid id)
        {
            await _requestRepository.Delete(id);
        }
    }
}