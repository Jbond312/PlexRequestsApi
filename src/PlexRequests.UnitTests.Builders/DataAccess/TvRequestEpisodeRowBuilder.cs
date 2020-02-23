using System;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class TvRequestEpisodeRowBuilder : IBuilder<TvRequestEpisodeRow>
    {
        private int _tvRequestEpisodeId;
        private string _title;
        private int _episodeIndex;
        private RequestStatuses _requestStatus;
        private string _imagePath;
        private DateTime? _airDateUtc;

        public TvRequestEpisodeRowBuilder()
        {
            _tvRequestEpisodeId = new Random().Next(1, int.MaxValue);
            _title = Guid.NewGuid().ToString();
            _episodeIndex = 1;
            _requestStatus = RequestStatuses.PendingApproval;
            _imagePath = Guid.NewGuid().ToString();
            _airDateUtc = null;
        }

        public TvRequestEpisodeRowBuilder WithId(int id)
        {
            _tvRequestEpisodeId = id;
            return this;
        }

        public TvRequestEpisodeRowBuilder WithEpisode(int episode)
        {
            _episodeIndex = episode;
            return this;
        }

        public TvRequestEpisodeRowBuilder WithRequestStatus(RequestStatuses requestStatus)
        {
            _requestStatus = requestStatus;
            return this;
        }

        public TvRequestEpisodeRow Build()
        {
            return new TvRequestEpisodeRow
            {
                TvRequestEpisodeId = _tvRequestEpisodeId,
                Title = _title,
                EpisodeIndex = _episodeIndex,
                RequestStatus = _requestStatus,
                ImagePath = _imagePath,
                AirDateUtc = _airDateUtc
            };
        }
    }
}
