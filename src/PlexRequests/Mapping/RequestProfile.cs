using AutoMapper;
using PlexRequests.Models.SubModels.Create;
using PlexRequests.Models.SubModels.Detail;
using PlexRequests.Repository.Models;

namespace PlexRequests.Mapping
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {
            CreateMap<Request, MovieRequestDetailModel>();
            CreateMap<RequestSeason, TvRequestSeasonDetailModel>();
            CreateMap<RequestEpisode, TvRequestEpisodeDetailModel>();
            CreateMap<TvRequestSeasonCreateModel, RequestSeason>();
            CreateMap<TvRequestEpisodeCreateModel, RequestEpisode>();
        }
    }
}