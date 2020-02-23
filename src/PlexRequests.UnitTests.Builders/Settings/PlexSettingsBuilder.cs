using PlexRequests.Core.Settings;

namespace PlexRequests.UnitTests.Builders.Settings
{
    public class PlexSettingsBuilder
    {
        private int _defaultLocalPort;
        private bool _connectLocally;
        private string _plexMediaUriFormat;

        public PlexSettingsBuilder()
        {
            _defaultLocalPort = 1000;
            _connectLocally = true;
            _plexMediaUriFormat = "https://app.plex.tv/desktop#!/server/{0}/details?key=%2Flibrary%2Fmetadata%2F{1}";
        }

        public PlexSettingsBuilder WithConnectLocally(bool connectLocally)
        {
            _connectLocally = connectLocally;
            return this;
        }

        public PlexSettings Build()
        {
            return new PlexSettings
            {
                DefaultLocalPort = _defaultLocalPort,
                ConnectLocally = _connectLocally,
                PlexMediaItemUriFormat = _plexMediaUriFormat
            };
        }
    }
}
