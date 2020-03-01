using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Functions.Features.Search.Models;
using PlexRequests.Plex.MediaItemRetriever;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.Functions.Features.Search.Helpers
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
                    searchModel.PlexMediaUri = plexMediaItem?.MediaUri;
                }
                else
                {
                    searchModel.RequestStatus = associatedRequest.RequestStatus;
                    searchModel.PlexMediaUri = associatedRequest.PlexMediaItem?.MediaUri;
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
                tvDetailModel.PlexMediaUri = plexMediaItem?.MediaUri;
            }
            else
            {
                tvDetailModel.RequestStatus = associatedRequest.RequestStatus;
                tvDetailModel.PlexMediaUri = associatedRequest.PlexMediaItem.MediaUri;
            }

            return tvDetailModel;
        }

        public async Task<TvSeasonDetailModel> CreateSeasonDetailModel(int tvId, TvSeasonDetails tvSeasonDetails)
        {
            var associatedRequestLookup = await _requestService.GetRequestsByMovieDbIds(new List<int> { tvSeasonDetails.Id });

            var tvSeasonDetailModel = _mapper.Map<TvSeasonDetailModel>(tvSeasonDetails);

            if (associatedRequestLookup.TryGetValue(tvId, out var associatedRequest))
            {
                var requestSeason = associatedRequest.TvRequestSeasons.FirstOrDefault(x => x.SeasonIndex == tvSeasonDetailModel.Index);

                if (requestSeason != null)
                {
                    tvSeasonDetailModel.RequestStatus = requestSeason.RequestStatus;
                }
            }

            var plexMediaItem = await _mediaItemRetriever.Get(tvId);
            var plexMediaSeason = plexMediaItem?.PlexSeasons?.FirstOrDefault(x => x.Season == tvSeasonDetailModel.Index);

            if (plexMediaSeason != null)
            {
                tvSeasonDetailModel.PlexMediaUri = plexMediaSeason.MediaUri;

                SetEpisodePlexMediaUris(tvSeasonDetailModel, plexMediaSeason);
            }

            return tvSeasonDetailModel;
        }

        private void SetEpisodePlexMediaUris(TvSeasonDetailModel tvSeasonDetailModel, PlexSeasonRow plexMediaSeason)
        {
            foreach (var episodeModel in tvSeasonDetailModel.Episodes)
            {
                if (string.IsNullOrEmpty(episodeModel.PlexMediaUri))
                {
                    var plexEpisode = plexMediaSeason.PlexEpisodes.FirstOrDefault(x => x.Episode == episodeModel.Index);

                    if (plexEpisode != null)
                    {
                        episodeModel.PlexMediaUri = plexEpisode.MediaUri;
                    }
                }
            }
        }
    }
}