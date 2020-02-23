using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class TvRequestSeasonRowBuilder : IBuilder<TvRequestSeasonRow>
    {
        private int _tvRequestSeasonId;
        private string _title;
        private int _seasonIndex;
        private RequestStatuses _requestStatus;
        private string _imagePath;
        private DateTime? _airDateUtc;
        private List<TvRequestEpisodeRowBuilder> _episodeRowBuilders;

        public TvRequestSeasonRowBuilder()
        {
            _tvRequestSeasonId = new Random().Next(1, int.MaxValue);
            _title = Guid.NewGuid().ToString();
            _seasonIndex = 1;
            _requestStatus = RequestStatuses.PendingApproval;
            _imagePath = Guid.NewGuid().ToString();
            _airDateUtc = null;
            _episodeRowBuilders = new List<TvRequestEpisodeRowBuilder>();

            WithTvRequestEpisodes();
        }

        public TvRequestSeasonRowBuilder WithId(int id)
        {
            _tvRequestSeasonId = id;
            return this;
        }

        public TvRequestSeasonRowBuilder WithSeason(int episode)
        {
            _seasonIndex = episode;
            return this;
        }

        public TvRequestSeasonRowBuilder WithRequestStatus(RequestStatuses requestStatus)
        {
            _requestStatus = requestStatus;
            return this;
        }

        public TvRequestSeasonRowBuilder WithTvRequestEpisodes(int count = 3)
        {
            for (var i = 0; i < count; i++)
            {
                _episodeRowBuilders.Add(new TvRequestEpisodeRowBuilder().WithId(i).WithEpisode(i));
            }

            return this;
        }

        public TvRequestSeasonRowBuilder WithTvRequestEpisode(TvRequestEpisodeRowBuilder tvRequestEpisodeBuilder)
        {
            _episodeRowBuilders.Add(tvRequestEpisodeBuilder);
            return this;
        }

        public TvRequestSeasonRow Build()
        {
            return new TvRequestSeasonRow
            {
                TvRequestSeasonId = _tvRequestSeasonId,
                Title = _title,
                SeasonIndex = _seasonIndex,
                RequestStatus = _requestStatus,
                ImagePath = _imagePath,
                AirDateUtc = _airDateUtc,
                TvRequestEpisodes = _episodeRowBuilders.Select(x => x.Build()).ToList()
            };
        }
    }
}
