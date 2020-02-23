namespace PlexRequests.Core.Helpers
{
    public interface IClaimsPrincipalAccessor
    {
        string Username { get; }
        int UserId { get; }
    }
}