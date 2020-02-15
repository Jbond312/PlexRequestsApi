using AutoMapper;
using PlexRequests.ApiRequests.Users.Models.Detail;
using PlexRequests.Models;

namespace PlexRequests.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDetailModel>();
        }
    }
}
