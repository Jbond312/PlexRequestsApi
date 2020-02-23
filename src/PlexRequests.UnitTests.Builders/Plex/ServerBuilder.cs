using System;
using PlexRequests.Plex.Models;

namespace PlexRequests.UnitTests.Builders.Plex
{
    public class ServerBuilder :IBuilder<Server>
    {
        private string _accessToken;
        private string _name;
        private string _address;
        private string _port;
        private string _scheme;
        private string _localAddress;
        private string _machineIdentifier;
        private string _owned;

        public ServerBuilder()
        {
            _accessToken = Guid.NewGuid().ToString();
            _name = Guid.NewGuid().ToString();
            _address = "111.222.333.444";
            _port = new Random().Next(1, int.MaxValue).ToString();
            _scheme = Guid.NewGuid().ToString();
            _localAddress = "192.168.0.1";
            _machineIdentifier = Guid.NewGuid().ToString();
            _owned = "0";
        }

        public Server Build()
        {
            return new Server
            {
                AccessToken = _accessToken,
                Name = _name,
                Address = _address,
                Port = _port,
                Scheme = _scheme,
                LocalAddresses = _localAddress,
                MachineIdentifier = _machineIdentifier,
                Owned = _owned
            };
        }
    }
}
