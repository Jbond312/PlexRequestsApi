using AutoMapper;
using PlexRequests.Models.ViewModels;
using PlexRequests.Store.Models;

namespace PlexRequests.Mapping
{
    public class PlexProfile : Profile
    {
        public PlexProfile()
        {
            CreateMap<PlexServer, PlexServerViewModel>();
        }
    }
}
