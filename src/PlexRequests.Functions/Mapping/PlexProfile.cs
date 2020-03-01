using AutoMapper;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Functions.Features.Plex.Models.Detail;

namespace PlexRequests.Functions.Mapping
{
    public class PlexProfile : Profile
    {
        public PlexProfile()
        {
            CreateMap<PlexServerRow, PlexServerDetailModel>()
                .ForMember(x =>x.Id, x => x.MapFrom(y => y.PlexServerId))
                .ForMember(x => x.Libraries, x => x.MapFrom(y => y.PlexLibraries));
            CreateMap<PlexLibraryRow, PlexServerLibraryDetailModel>()
                .ForMember(x => x.Key, x => x.MapFrom(y => y.LibraryKey));
        }
    }
}
