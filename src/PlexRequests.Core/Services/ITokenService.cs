using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
