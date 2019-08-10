using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Core.Helpers;
using PlexRequests.Repository;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IRequestHelper _requestHelper;

        public RequestService(
            IRequestRepository requestRepository,
            IRequestHelper requestHelper
            )
        {
            _requestRepository = requestRepository;
            _requestHelper = requestHelper;
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
            return await _requestRepository.GetMany(x =>
                x.MediaType == mediaType &&
                x.Status != RequestStatuses.Completed && x.PlexMediaUri == null);
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

        public RequestStatuses CalculateAggregatedStatus(Request request)
        {
            return _requestHelper.CalculateAggregatedStatus(request);
        }
    }
}