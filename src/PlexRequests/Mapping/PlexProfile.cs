using AutoMapper;
using PlexRequests.Models;
using PlexRequests.Store.Models;

namespace PlexRequests.Mapping
{
    public class PlexProfile : Profile
    {
        public PlexProfile()
        {
            CreateMap<PlexServer, PlexServerModel>();
        }
    }
}
