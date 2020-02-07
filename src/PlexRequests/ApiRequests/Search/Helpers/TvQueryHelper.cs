using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.Core.Services;
using PlexRequests.Plex.MediaItemRetriever;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.ApiRequests.Search.Helpers
{
    public class TvQueryHelper : ITvQueryHelper
    {
        private readonly IMapper _mapper;
        private readonly ITvRequestService _requestService;
        private readonly IMediaItemRetriever _mediaItemRetriever;

        public TvQueryHelper(
            IMapper mapper,
            ITvRequestService requestService,
            IEnumerable<IMediaItemRetriever> mediaItemRetrievers
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _mediaItemRetriever = mediaItemRetrievers.First(x => x.MediaType == PlexMediaTypes.Show);
        }

        public async Task<List<TvSearchModel>> CreateSearchModels(List<TvSearch> queryResults)
        {
            var movieDbIds = queryResults.Select(x => x.Id).ToList();

            var requests = await _requestService.GetRequestsByMovieDbIds(movieDbIds);

            var searchModels = _mapper.Map<List<TvSearchModel>>(queryResults);

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

        public async Task<TvDetailModel> CreateShowDetailModel(TvDetails tvDetails)
        {
            var associatedRequestLookup = await _requestService.GetRequestsByMovieDbIds(new List<int> { tvDetails.Id });

            var tvDetailModel = _mapper.Map<TvDetailModel>(tvDetails);

            if (!associatedRequestLookup.TryGetValue(tvDetailModel.Id, out var associatedRequest))
            {
                var plexMediaItem = await _mediaItemRetriever.Get(tvDetailModel.Id);
                tvDetailModel.PlexMediaUri = plexMediaItem?.PlexMediaUri;
            }
            else
            {
                tvDetailModel.RequestStatus = associatedRequest.Status;
                tvDetailModel.PlexMediaUri = associatedRequest.PlexMediaUri;
            }

            return tvDetailModel;
        }

        public async Task<TvSeasonDetailModel> CreateSeasonDetailModel(int tvId, TvSeasonDetails tvSeasonDetails)
        {
            var associatedRequestLookup = await _requestService.GetRequestsByMovieDbIds(new List<int> { tvSeasonDetails.Id });

            var tvSeasonDetailModel = _mapper.Map<TvSeasonDetailModel>(tvSeasonDetails);

            if (associatedRequestLookup.TryGetValue(tvId, out var associatedRequest))
            {
                var requestSeason = associatedRequest.Seasons.FirstOrDefault(x => x.Index == tvSeasonDetailModel.Index);

                if (requestSeason != null)
                {
                    tvSeasonDetailModel.RequestStatus = requestSeason.Status;
                }
            }

            var plexMediaItem = await _mediaItemRetriever.Get(tvId);
            var plexMediaSeason = plexMediaItem?.Seasons?.FirstOrDefault(x => x.Season == tvSeasonDetailModel.Index);

            if (plexMediaSeason != null)
            {
                tvSeasonDetailModel.PlexMediaUri = plexMediaSeason.PlexMediaUri;

                SetEpisodePlexMediaUris(tvSeasonDetailModel, plexMediaSeason);
            }

            return tvSeasonDetailModel;
        }

        private void SetEpisodePlexMediaUris(TvSeasonDetailModel tvSeasonDetailModel, PlexSeason plexMediaSeason)
        {
            foreach (var episodeModel in tvSeasonDetailModel.Episodes)
            {
                if (string.IsNullOrEmpty(episodeModel.PlexMediaUri))
                {
                    var plexEpisode = plexMediaSeason.Episodes.FirstOrDefault(x => x.Episode == episodeModel.Index);

                    if (plexEpisode != null)
                    {
                        episodeModel.PlexMediaUri = plexEpisode.PlexMediaUri;
                    }
                }
            }
        }
    }
}