using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Core.Helpers;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.DataAccess.Repositories;

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

        public async Task<TvRequestRow> GetRequestById(int id)
        {
            return await _requestRepository.GetOne(x => x.TvRequestId == id);
        }

        public async Task<Paged<TvRequestRow>> GetPaged(string title, RequestStatuses? status, int? userId, int? page, int? pageSize)
        {
            return await _requestRepository.GetPaged(title, status, userId, page, pageSize);
        }

        public async Task<List<TvRequestRow>> GetExistingRequests(AgentTypes agentType, string agentSourceId)
        {
            return await _requestRepository.GetMany(x => x.PrimaryAgent.AgentType == agentType && x.PrimaryAgent.AgentSourceId == agentSourceId);
        }

        public async Task<List<TvRequestRow>> GetIncompleteRequests()
        {
            return await _requestRepository.GetMany(x => x.RequestStatus != RequestStatuses.Completed && x.PlexMediaItem.MediaUri == null);
        }

        public async Task<Dictionary<int, TvRequestRow>> GetRequestsByMovieDbIds(List<int> moviedbIds)
        {
            var idsAsSourceIds = moviedbIds.Select(x => x.ToString()).ToList();
            var requests = await _requestRepository.GetMany(x => idsAsSourceIds.Contains(x.PrimaryAgent.AgentSourceId));
            return requests.ToDictionary(x => int.Parse(x.PrimaryAgent.AgentSourceId), x => x);
        }

        public async Task Add(TvRequestRow request)
        {
            await _requestRepository.Add(request);
        }

        public async Task DeleteRequest(int id)
        {
            await _requestRepository.Delete(id);
        }

        public void SetAggregatedStatus(TvRequestRow request)
        {
            _requestHelper.SetAggregatedStatus(request);
        }
    }
}