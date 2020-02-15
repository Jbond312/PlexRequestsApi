using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Core.ExtensionMethods
{
    public static class PlexServerExtensionMethods
    {
        public static string GetPlexUri(this PlexServerRow server, bool useLocal)
        {
            var uri = useLocal ? $"{server.Scheme}://{server.LocalIp}:{server.LocalPort}" : $"{server.Scheme}://{server.ExternalIp}:{server.ExternalPort}";

            return uri;
        }
    }
}
