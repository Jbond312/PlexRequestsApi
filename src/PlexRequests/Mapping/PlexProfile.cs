using AutoMapper;
using PlexRequests.Models.SubModels.Detail;
using PlexRequests.Store.Models;

namespace PlexRequests.Mapping
{
    public class PlexProfile : Profile
    {
        public PlexProfile()
        {
            CreateMap<PlexServer, PlexServerDetailModel>();
        }
    }
}
