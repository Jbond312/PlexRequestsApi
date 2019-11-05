using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Repository;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public class MovieRequestService : IMovieRequestService
    {
        private readonly IMovieRequestRepository _requestRepository;

        public MovieRequestService(
            IMovieRequestRepository requestRepository
            )
        {
            _requestRepository = requestRepository;
        }

        public async Task<MovieRequest> GetRequestById(Guid id)
        {
            return await _requestRepository.GetOne(x => x.Id == id);
        }

        public async Task<Paged<MovieRequest>> GetPaged(string title, RequestStatuses? status, Guid? userId, int? page, int? pageSize)
        {
            return await _requestRepository.GetPaged(title, status, userId, page, pageSize);
        }

        public async Task<MovieRequest> GetExistingRequest(AgentTypes agentType, string agentSourceId)
        {
            return await _requestRepository.GetOne(x => x.PrimaryAgent.AgentType == agentType && x.PrimaryAgent.AgentSourceId == agentSourceId);
        }

        public async Task<List<MovieRequest>> GetIncompleteRequests()
        {
            return await _requestRepository.GetMany(x => x.Status != RequestStatuses.Completed && x.PlexMediaUri == null);
        }

        public async Task<Dictionary<int, MovieRequest>> GetRequestsByMovieDbIds(List<int> moviedbIds)
        {
            var idsAsSourceIds = moviedbIds.Select(x => x.ToString()).ToList();
            var requests = await _requestRepository.GetManyIn<string>(x => x.PrimaryAgent.AgentSourceId, idsAsSourceIds);
            return requests.ToDictionary(x => int.Parse(x.PrimaryAgent.AgentSourceId), x => x);
        }

        public async Task Create(MovieRequest request)
        {
            await _requestRepository.Create(request);
        }

        public async Task Update(MovieRequest request)
        {
            await _requestRepository.Update(request);
        }

        public async Task DeleteRequest(Guid id)
        {
            await _requestRepository.Delete(id);
        }
    }
}