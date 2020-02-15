using AutoMapper;
using PlexRequests.ApiRequests.Plex.Models.Detail;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Mapping
{
    public class PlexProfile : Profile
    {
        public PlexProfile()
        {
            CreateMap<PlexServerRow, PlexServerDetailModel>()
                .ForMember(x =>x.Id, x => x.MapFrom(y => y.PlexServerId));
            CreateMap<PlexLibraryRow, PlexServerLibraryDetailModel>()
                .ForMember(x => x.Key, x => x.MapFrom(y => y.LibraryKey));
        }
    }
}
