using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.Core.Services;
using PlexRequests.Plex.MediaItemRetriever;
using PlexRequests.Repository.Enums;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.ApiRequests.Search.Helpers
{
    public class MovieQueryHelper : IMovieQueryHelper
    {
        private readonly IMapper _mapper;
        private readonly IMovieRequestService _requestService;
        private readonly IMediaItemRetriever _mediaItemRetriever;

        public MovieQueryHelper(
            IMapper mapper,
            IMovieRequestService requestService,
            IEnumerable<IMediaItemRetriever> mediaItemRetrievers
            )
        {
            _mapper = mapper;
            _requestService = requestService;
            _mediaItemRetriever = mediaItemRetrievers.First(x => x.MediaType == PlexMediaTypes.Movie);
        }

        public async Task<List<MovieSearchModel>> CreateSearchModels(List<MovieSearch> queryResults)
        {
            var movieDbIds = queryResults.Select(x => x.Id).ToList();

            var requests = await _requestService.GetRequestsByMovieDbIds(movieDbIds);

            var searchModels = _mapper.Map<List<MovieSearchModel>>(queryResults);

            foreach (var searchModel in searchModels)
            {
                if (!requests.TryGetValue(searchModel.Id, out var associatedRequest))
                {
                    var plexMediaItem = await _mediaItemRetriever.Get(searchModel.Id);
                    searchModel.PlexMediaUri = plexMediaItem?.PlexMediaUri;
                }
                else
                {
                    searchModel.RequestStatus = associatedRequest.Status;
                    searchModel.PlexMediaUri = associatedRequest.PlexMediaUri;
                }
            }

            return searchModels;
        }

        public async Task<MovieDetailModel> CreateDetailModel(MovieDetails movieDetails)
        {
            var associatedRequestLookup = await _requestService.GetRequestsByMovieDbIds(new List<int> { movieDetails.Id });

            var movieDetailModel = _mapper.Map<MovieDetailModel>(movieDetails);

            if (!associatedRequestLookup.TryGetValue(movieDetailModel.Id, out var associatedRequest))
            {
                var plexMediaItem = await _mediaItemRetriever.Get(movieDetailModel.Id);
                movieDetailModel.PlexMediaUri = plexMediaItem?.PlexMediaUri;
            }
            else
            {
                movieDetailModel.RequestStatus = associatedRequest.Status;
                movieDetailModel.PlexMediaUri = associatedRequest.PlexMediaUri;
            }

            return movieDetailModel;
        }
    }
}
