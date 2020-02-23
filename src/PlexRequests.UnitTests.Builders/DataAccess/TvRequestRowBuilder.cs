using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class TvRequestRowBuilder : IBuilder<TvRequestRow>
    {
        private int _tvRequestId;
        private int _theMovieDbId;
        private string _title;
        private RequestStatuses _requestStatus;
        private string _imagePath;
        private DateTime? _airDateUtc;
        private bool _track;
        private string _comment;
        private List<TvRequestSeasonRowBuilder> _tvRequestSeasonBuilders;
        private List<TvRequestUserRowBuilder> _tvRequestUserBuilders;
        private List<TvRequestAgentRowBuilder> _tvRequestAgentBuilders;
        private TvPlexMediaItemRowBuilder _mediaItemBuilder;


        public TvRequestRowBuilder()
        {
            _tvRequestId = new Random().Next(1, int.MaxValue);
            _theMovieDbId = new Random().Next(1, int.MaxValue);
            _title = Guid.NewGuid().ToString();
            _requestStatus = RequestStatuses.PendingApproval;
            _imagePath = Guid.NewGuid().ToString();
            _airDateUtc = null;
            _track = false;
            _comment = null;
            _tvRequestSeasonBuilders = new List<TvRequestSeasonRowBuilder>();
            _tvRequestUserBuilders = new List<TvRequestUserRowBuilder>();
            _tvRequestAgentBuilders = new List<TvRequestAgentRowBuilder>();
            _mediaItemBuilder = new TvPlexMediaItemRowBuilder();

            WithTvRequestSeasons();
            WithTvRequestUsers();
            WithTvRequestAgents();
        }

        public TvRequestRowBuilder WithId(int id)
        {
            _tvRequestId = id;
            return this;
        }

        public TvRequestRowBuilder WithRequestStatus(RequestStatuses requestStatus)
        {
            _requestStatus = requestStatus;
            return this;
        }

        public TvRequestRowBuilder WithTrack(bool track)
        {
            _track = track;
            return this;
        }

        public TvRequestRowBuilder WithComment(string comment)
        {
            _comment = comment;
            return this;
        }

        public TvRequestRowBuilder WithTvRequestSeasons(int count = 3)
        {
            for (var i = 0; i < count; i++)
            {
                _tvRequestSeasonBuilders.Add(new TvRequestSeasonRowBuilder().WithId(i).WithSeason(i));
            }
            return this;
        }

        public TvRequestRowBuilder WithTvRequestSeason(TvRequestSeasonRowBuilder tvRequestSeasonRowBuilder)
        {
           _tvRequestSeasonBuilders.Add(tvRequestSeasonRowBuilder);
            return this;
        }

        public TvRequestRowBuilder WithTvRequestUsers(int count = 3)
        {
            for (var i = 0; i < count; i++)
            {
                _tvRequestUserBuilders.Add(new TvRequestUserRowBuilder().WithId(i));
            }
            return this;
        }

        public TvRequestRowBuilder WithTvRequestUser(TvRequestUserRowBuilder tvRequestUserBuilder)
        {
            _tvRequestUserBuilders.Add(tvRequestUserBuilder);
            return this;
        }

        public TvRequestRowBuilder WithTvRequestAgents(int count = 3)
        {
            for (var i = 0; i < count; i++)
            {
                _tvRequestAgentBuilders.Add(new TvRequestAgentRowBuilder());
            }
            return this;
        }

        public TvRequestRowBuilder WithTvRequestAgent(TvRequestAgentRowBuilder tvRequestAgentBuilder)
        {
            _tvRequestAgentBuilders.Add(tvRequestAgentBuilder);
            return this;
        }

        public TvRequestRowBuilder WithMediaItem(TvPlexMediaItemRowBuilder mediaItemBuilder)
        {
            _mediaItemBuilder = mediaItemBuilder;
            return this;
        }

        public TvRequestRow Build()
        {
            var mediaItem = _mediaItemBuilder.Build();
            return new TvRequestRow
            {
                TvRequestId = _tvRequestId,
                TheMovieDbId = _theMovieDbId,
                Title = _title,
                RequestStatus = _requestStatus,
                ImagePath = _imagePath,
                AirDateUtc = _airDateUtc,
                Track = _track,
                Comment = _comment,
                PlexMediaItem = mediaItem,
                PlexMediaItemId = mediaItem.PlexMediaItemId,
                TvRequestSeasons = _tvRequestSeasonBuilders.Select(x => x.Build()).ToList(),
                TvRequestUsers = _tvRequestUserBuilders.Select(x => x.Build()).ToList(),
                TvRequestAgents = _tvRequestAgentBuilders.Select(x => x.Build()).ToList()
            };
        }
    }
}
