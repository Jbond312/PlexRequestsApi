using System;

namespace PlexRequests.Core.Helpers
{
    public interface IClaimsPrincipalAccessor
    {
        string Username { get; }
        Guid UserId { get; }
    }
}