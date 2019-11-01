using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Core.Helpers;
using PlexRequests.Repository;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public class TvRequestService : ITvRequestService
    {
        private readonly ITvRequestRepository _requestRepository;
        private readonly IRequestHelper _requestHelper;

        public TvRequestService(
            ITvRequestRepository requestRepository,
            IRequestHelper requestHelper
            )
        {
            _requestRepository = requestRepository;
            _requestHelper = requestHelper;
        }

        public async Task<TvRequest> GetRequestById(Guid id)
        {
            return await _requestRepository.GetOne(x => x.Id == id);
        }

        public async Task<Paged<TvRequest>> GetPaged(string title, RequestStatuses? status, Guid? userId, int? page, int? pageSize)
        {
            return await _requestRepository.GetPaged(title, status, userId, page, pageSize);
        }

        public async Task<List<TvRequest>> GetExistingRequests(AgentTypes agentType, string agentSourceId)
        {
            return await _requestRepository.GetMany(x => x.PrimaryAgent.AgentType == agentType && x.PrimaryAgent.AgentSourceId == agentSourceId);
        }

        public async Task<List<TvRequest>> GetIncompleteRequests()
        {
            return await _requestRepository.GetMany(x => x.Status != RequestStatuses.Completed && x.PlexMediaUri == null);
        }

        public async Task<Dictionary<int, TvRequest>> GetRequestsByMovieDbIds(List<int> moviedbIds)
        {
            //TODO Don't get all of the db entities here. Get the mongo filter working
            var requests = await _requestRepository.GetMany();
            var matchingRequests = requests.Where(x => moviedbIds.Contains(int.Parse(x.PrimaryAgent.AgentSourceId))).ToList();
            return matchingRequests.ToDictionary(x => int.Parse(x.PrimaryAgent.AgentSourceId), x => x);
        }

        public async Task Create(TvRequest request)
        {
            await _requestRepository.Create(request);
        }

        public async Task Update(TvRequest request)
        {
            await _requestRepository.Update(request);
        }

        public async Task DeleteRequest(Guid id)
        {
            await _requestRepository.Delete(id);
        }

        public void SetAggregatedStatus(TvRequest request)
        {
            _requestHelper.SetAggregatedStatus(request);
        }
    }
}