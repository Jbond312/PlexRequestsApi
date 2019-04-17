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

        public async Task<Paged<Request>> GetPaged(string title, PlexMediaTypes? mediaType, RequestStatuses? status, Guid? userId, int? page, int? pageSize)
        {
            return await _requestRepository.GetPaged(title, mediaType, status, userId, page, pageSize);
        }

        public async Task<Request> GetExistingMovieRequest(AgentTypes agentType, string agentSourceId)
        {
            return await _requestRepository.GetOne(x =>
                x.MediaType == PlexMediaTypes.Movie && x.PrimaryAgent.AgentType == agentType && x.PrimaryAgent.AgentSourceId == agentSourceId);
        }

        public async Task<List<Request>> GetExistingTvRequests(AgentTypes agentType, string agentSourceId)
        {
            return await _requestRepository.GetMany(x =>
                x.MediaType == PlexMediaTypes.Show && x.PrimaryAgent.AgentType == agentType && x.PrimaryAgent.AgentSourceId == agentSourceId);
        }

        public async Task<List<Request>> GetIncompleteRequests(PlexMediaTypes mediaType)
        {
            var validRequestStatuses = new List<RequestStatuses>
            {
                RequestStatuses.Approved,
                RequestStatuses.PendingApproval,
                RequestStatuses.PartialCompletion
            };
            
            return await _requestRepository.GetMany(x =>
                x.MediaType == mediaType && validRequestStatuses.Contains(x.Status)
                                         && x.PlexMediaUri == null);
        }

        public async Task Create(Request request)
        {
            await _requestRepository.Create(request);
        }

        public async Task Update(Request request)
        {
            await _requestRepository.Update(request);
        }

        public async Task DeleteRequest(Guid id)
        {
            await _requestRepository.Delete(id);
        }
    }
}