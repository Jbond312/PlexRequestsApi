using System;
using PlexRequests.Plex.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class UserBuilder : IBuilder<User>
    {
        private int _id;
        private string _email;
        private string _userName;
        private string _title;
        private string _authToken;

        public UserBuilder()
        {
            _id = new Random().Next(1, int.MaxValue);
            _email = Guid.NewGuid().ToString();
            _userName = Guid.NewGuid().ToString();
            _title = Guid.NewGuid().ToString();
            _authToken = Guid.NewGuid().ToString();
        }

        public User Build()
        {
            return new User
            {
                Id = _id,
                Email = _email,
                Username = _userName,
                Title = _title,
                AuthToken = _authToken,
            };
        }
    }
}
