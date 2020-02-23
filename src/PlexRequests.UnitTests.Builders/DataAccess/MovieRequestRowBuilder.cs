using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class MovieRequestRowBuilder : IBuilder<MovieRequestRow>
    {
        private int _movieRequestId;
        private int _theMovieDbId;
        private string _title;
        private RequestStatuses _requestStatus;
        private string _imagePath;
        private DateTime? _airDateUtc;
        private string _comment;
        private List<MovieRequestAgentRowBuilder> _movieRequestAgentBuilders;
        private UserRowBuilder _userBuilder;
        private MoviePlexMediaItemRowBuilder _mediaItemBuilder;


        public MovieRequestRowBuilder()
        {
            _movieRequestId = new Random().Next(1, int.MaxValue);
            _theMovieDbId = new Random().Next(1, int.MaxValue);
            _title = Guid.NewGuid().ToString();
            _requestStatus = RequestStatuses.PendingApproval;
            _imagePath = Guid.NewGuid().ToString();
            _airDateUtc = null;
            _comment = null;
            _movieRequestAgentBuilders = new List<MovieRequestAgentRowBuilder>();
            _userBuilder = new UserRowBuilder();
            _mediaItemBuilder = new MoviePlexMediaItemRowBuilder();
        }

        public MovieRequestRowBuilder WithId(int id)
        {
            _movieRequestId = id;
            return this;
        }

        public MovieRequestRowBuilder WithRequestStatus(RequestStatuses requestStatus)
        {
            _requestStatus = requestStatus;
            return this;
        }

        public MovieRequestRowBuilder WithComment(string comment)
        {
            _comment = comment;
            return this;
        }

        public MovieRequestRowBuilder WithMovieRequestAgents(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _movieRequestAgentBuilders.Add(new MovieRequestAgentRowBuilder());
            }
            return this;
        }

        public MovieRequestRowBuilder WithMovieRequestAgent(MovieRequestAgentRowBuilder movieRequestAgentBuilder)
        {
            _movieRequestAgentBuilders.Add(movieRequestAgentBuilder);
            return this;
        }

        public MovieRequestRowBuilder WithUser(UserRowBuilder userBuilder)
        {
            _userBuilder = userBuilder;
            return this;
        }

        public MovieRequestRowBuilder WithMediaItem(MoviePlexMediaItemRowBuilder moviePlexMediaItemBuilder)
        {
            _mediaItemBuilder = moviePlexMediaItemBuilder;
            return this;
        }

        public MovieRequestRow Build()
        {
            var user = _userBuilder.Build();
            var plexMediaItem = _mediaItemBuilder.Build();
            return new MovieRequestRow
            {
                MovieRequestId = _movieRequestId,
                TheMovieDbId = _theMovieDbId,
                Title = _title,
                RequestStatus = _requestStatus,
                ImagePath = _imagePath,
                AirDateUtc = _airDateUtc,
                Comment = _comment,
                MovieRequestAgents = _movieRequestAgentBuilders.Select(x => x.Build()).ToList(),
                UserId = user.UserId,
                User = user,
                PlexMediaItem = plexMediaItem,
                PlexMediaItemId = plexMediaItem.PlexMediaItemId
            };
        }
    }
}
