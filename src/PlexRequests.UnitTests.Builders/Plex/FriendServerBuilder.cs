using System;
using PlexRequests.Plex.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class FriendServerBuilder : IBuilder<FriendServer>
    {
        private string _id;
        private string _machineIdentifier;
        private string _name;
        private string _lastSeenAt;
        private string _numLibraries;
        private string _owned;
        private string _serverId;

        public FriendServerBuilder()
        {
            _id = Guid.NewGuid().ToString();
            _machineIdentifier = Guid.NewGuid().ToString();
            _name = Guid.NewGuid().ToString();
            _lastSeenAt = Guid.NewGuid().ToString();
            _numLibraries = Guid.NewGuid().ToString();
            _owned = Guid.NewGuid().ToString();
            _serverId = Guid.NewGuid().ToString();
        }

        public FriendServer Build()
        {
            return new FriendServer
            {
                Id = _id,
                MachineIdentifier = _machineIdentifier,
                Name = _name,
                LastSeenAt = _lastSeenAt,
                NumLibraries = _numLibraries,
                Owned = _owned,
                ServerId = _serverId
            };
        }
    }
}
