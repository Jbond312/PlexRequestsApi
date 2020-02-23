using System;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class TvRequestUserRowBuilder : IBuilder<TvRequestUserRow>
    {
        private int _tvRequestUserId;
        private int? _season;
        private int? _episode;
        private bool _track;
        private UserRowBuilder _userBuilder;

        public TvRequestUserRowBuilder()
        {
            _tvRequestUserId = new Random().Next(1, int.MaxValue);
            _season = new Random().Next(1, int.MaxValue);
            _episode = new Random().Next(1, int.MaxValue);
            _track = false;
            _userBuilder = new UserRowBuilder();
        }

        public TvRequestUserRowBuilder WithId(int id)
        {
            _tvRequestUserId = id;
            return this;
        }

        public TvRequestUserRowBuilder WithSeason(int season)
        {
            _season = season;
            return this;
        }

        public TvRequestUserRowBuilder WithEpisode(int episode)
        {
            _episode = episode;
            return this;
        }

        public TvRequestUserRowBuilder WithTrack(bool track)
        {
            _track = track;
            return this;
        }

        public TvRequestUserRowBuilder WithUser(UserRowBuilder userBuilder)
        {
            _userBuilder = userBuilder;
            return this;
        }

        public TvRequestUserRowBuilder WithUserId(int userId)
        {
            _userBuilder.WithUserId(userId);
            return this;
        }

        public TvRequestUserRow Build()
        {
            var user = _userBuilder.Build();

            return new TvRequestUserRow
            {
                TvRequestUserId = _tvRequestUserId,
                Season = _season,
                Episode = _episode,
                Track = _track,
                User = user,
                UserId = user.UserId
            };
        }
    }
}
