using Microsoft.AspNetCore.Http;

namespace PlexRequests.Functions
{
    public static class QueryCollectionExtensions
    {
        public static bool TryParseInt(this IQueryCollection queryCollection, string paramName, out int result)
        {
            result = 0;
            if (!queryCollection.TryGetValue(paramName, out var paramValue))
            {
                return false;
            }

            if (!int.TryParse(paramValue, out var parsedResult))
            {
                return false;
            }

            result = parsedResult;

            return true;
        }
    }
}
