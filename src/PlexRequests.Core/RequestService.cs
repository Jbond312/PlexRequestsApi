using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        
        public async Task<List<Request>> GetMany(Expression<Func<Request, bool>> filter = null)
        {
            return await _requestRepository.GetMany(filter);
        }

        public async Task<Request> GetOne(Expression<Func<Request, bool>> filter = null)
        {
            return await _requestRepository.GetOne(filter);
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
    }
}