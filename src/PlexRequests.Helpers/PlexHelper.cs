namespace PlexRequests.Helpers
{
    public class PlexHelper
    {
        public static string GenerateMediaItemUri(string uriFormat, string machineIdentifier, int mediaItemKey)
        {
            return string.Format(uriFormat, machineIdentifier, mediaItemKey);
        }
    }
}