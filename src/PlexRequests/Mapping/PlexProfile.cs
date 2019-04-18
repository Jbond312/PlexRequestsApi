using AutoMapper;
using PlexRequests.ApiRequests.Plex.DTOs.Detail;
using PlexRequests.Repository.Models;

namespace PlexRequests.Mapping
{
    public class PlexProfile : Profile
    {
        public PlexProfile()
        {
            CreateMap<PlexServer, PlexServerDetailModel>();
            CreateMap<PlexServerLibrary, PlexServerLibraryDetailModel>();
        }
    }
}
