using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
