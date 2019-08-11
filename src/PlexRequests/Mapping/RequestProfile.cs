using AutoMapper;
using PlexRequests.ApiRequests.Requests.DTOs.Create;
using PlexRequests.ApiRequests.Requests.DTOs.Detail;
using PlexRequests.Repository.Models;

namespace PlexRequests.Mapping
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {
            CreateMap<Request, MovieRequestDetailModel>();
            CreateMap<Request, TvRequestDetailModel>()
            .ForMember(x => x.TrackShow, x => x.MapFrom(y => y.Track));
            CreateMap<RequestSeason, TvRequestSeasonDetailModel>();
            CreateMap<RequestEpisode, TvRequestEpisodeDetailModel>();
            CreateMap<TvRequestSeasonCreateModel, RequestSeason>();
            CreateMap<TvRequestEpisodeCreateModel, RequestEpisode>();
        }
    }
}