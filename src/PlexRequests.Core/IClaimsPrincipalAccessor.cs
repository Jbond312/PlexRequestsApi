using System;

namespace PlexRequests.Core
{
    public interface IClaimsPrincipalAccessor
    {
        string Username { get; }
        Guid UserId { get; }
    }
}