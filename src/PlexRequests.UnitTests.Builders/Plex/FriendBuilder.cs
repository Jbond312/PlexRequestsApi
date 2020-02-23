using System;
using PlexRequests.Plex.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class FriendBuilder : IBuilder<Friend>
    {
        private string _id;
        private string _title;
        private string _email;
        private string _userName;
        private string _recommendationsPlaylistId;
        private FriendServerBuilder _friendServer;
        private string _thumb;

        public FriendBuilder()
        {
            _id = new Random().Next(1, int.MaxValue).ToString();
            _title = Guid.NewGuid().ToString();
            _email = Guid.NewGuid().ToString();
            _userName = Guid.NewGuid().ToString();
            _recommendationsPlaylistId = Guid.NewGuid().ToString();
            _friendServer = new FriendServerBuilder();
            _thumb = Guid.NewGuid().ToString();
        }

        public FriendBuilder WithServer(FriendServerBuilder friendServerBuilder)
        {
            _friendServer = friendServerBuilder;
            return this;
        }

        public Friend Build()
        {
            return new Friend
            {
                Id = _id,
                Title = _title,
                Email = _email,
                Username = _userName,
                RecommendationsPlaylistId = _recommendationsPlaylistId,
                Server = _friendServer.Build(),
                Thumb = _thumb,
            };
        }
    }
}
