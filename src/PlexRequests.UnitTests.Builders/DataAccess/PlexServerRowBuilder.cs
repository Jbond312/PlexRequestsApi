using System;
using System.Collections.Generic;
using System.Linq;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class PlexServerRowBuilder : IBuilder<PlexServerRow>
    {
        private int _plexServerId;
        private Guid _identifier;
        private string _name;
        private string _accessToken;
        private string _machineIdentifier;
        private string _scheme;
        private string _localIp;
        private int _localPort;
        private string _externalIp;
        private int _externalPort;
        private List<PlexLibraryRowBuilder> _libraries;

        public PlexServerRowBuilder()
        {
            _plexServerId = 1;
            _identifier = Guid.NewGuid();
            _name = Guid.NewGuid().ToString();
            _accessToken = Guid.NewGuid().ToString();
            _machineIdentifier = Guid.NewGuid().ToString();
            _scheme = "Scheme";
            _localIp = "192.168.0.1";
            _localPort = 1000;
            _externalIp = "81.155.67.122";
            _externalPort = 34000;
            _libraries = new List<PlexLibraryRowBuilder>();
        }

        public PlexServerRowBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public PlexServerRowBuilder WithLibraries(int libraryCount = 1)
        {
            for (var i = 0; i < libraryCount; i++)
            {
                _libraries.Add(new PlexLibraryRowBuilder());
            }

            return this;
        }

        public PlexServerRowBuilder WithLibrary(PlexLibraryRowBuilder libraryBuilder)
        {
            _libraries.Add(libraryBuilder);
            return this;
        }

        public PlexServerRow Build()
        {
            return new PlexServerRow
            {
                PlexServerId = _plexServerId,
                Identifier = _identifier,
                Name = _name,
                AccessToken = _accessToken,
                MachineIdentifier = _machineIdentifier,
                Scheme = _scheme,
                LocalIp = _localIp,
                LocalPort = _localPort,
                ExternalIp = _externalIp,
                ExternalPort = _externalPort,
                PlexLibraries = _libraries.Select(x => x.Build()).ToList()
            };
        }
    }
}
