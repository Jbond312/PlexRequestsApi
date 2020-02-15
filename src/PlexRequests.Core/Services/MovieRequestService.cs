using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.DataAccess.Repositories;

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

        public async Task<MovieRequestRow> GetRequestById(int id)
        {
            return await _requestRepository.GetOne(x => x.MovieRequestId == id);
        }

        public async Task<Paged<MovieRequestRow>> GetPaged(string title, RequestStatuses? status, int? userId, int? page, int? pageSize)
        {
            return await _requestRepository.GetPaged(title, status, userId, page, pageSize);
        }

        public async Task<MovieRequestRow> GetExistingRequest(AgentTypes agentType, string agentSourceId)
        {
            return await _requestRepository.GetOne(x => x.PrimaryAgent.AgentType == agentType && x.PrimaryAgent.AgentSourceId == agentSourceId);
        }

        public async Task<List<MovieRequestRow>> GetIncompleteRequests()
        {
            return await _requestRepository.GetMany(x => x.RequestStatus != RequestStatuses.Completed && x.PlexMediaItem.MediaUri == null);
        }

        public async Task<Dictionary<int, MovieRequestRow>> GetRequestsByMovieDbIds(List<int> moviedbIds)
        {
            var idsAsSourceIds = moviedbIds.Select(x => x.ToString()).ToList();
            var requests = await _requestRepository.GetMany(x => idsAsSourceIds.Contains(x.PrimaryAgent.AgentSourceId));
            return requests.ToDictionary(x => int.Parse(x.PrimaryAgent.AgentSourceId), x => x);
        }

        public async Task Add(MovieRequestRow request)
        {
            await _requestRepository.Add(request);
        }

        public async Task DeleteRequest(int id)
        {
            await _requestRepository.Delete(id);
        }
    }
}