using AutoMapper;
using PlexRequests.ApiRequests.Requests.Models.Create;
using PlexRequests.ApiRequests.Requests.Models.Detail;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Mapping
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {
            CreateMap<MovieRequestRow, MovieRequestDetailModel>()
                .ForMember(x => x.Id, x => x.MapFrom(y => y.MovieRequestId))
                .ForMember(x => x.AirDate, x => x.MapFrom(y => y.AirDateUtc))
                .ForMember(x => x.ImagePath, x => x.MapFrom(y => y.ImagePath))
                .ForMember(x => x.PlexMediaUri, x => x.MapFrom(y => y.PlexMediaItem.MediaUri));
            CreateMap<TvRequestRow, TvRequestDetailModel>()
            .ForMember(x => x.TrackShow, x => x.MapFrom(y => y.Track))
            .ForMember(x => x.AirDate, x => x.MapFrom(y => y.AirDateUtc))
            .ForMember(x => x.ImagePath, x => x.MapFrom(y => y.ImagePath))
            .ForMember(x => x.PlexMediaUri, x => x.MapFrom(y => y.PlexMediaItem.MediaUri));
            CreateMap<TvRequestSeasonRow, TvRequestSeasonDetailModel>()
                .ForMember(x => x.AirDate, x => x.MapFrom(y => y.AirDateUtc))
                .ForMember(x => x.PlexMediaUri, x => x.MapFrom(y => y.PlexSeason.MediaUri));
            CreateMap<TvRequestEpisodeRow, TvRequestEpisodeDetailModel>()
                .ForMember(x => x.AirDate, x => x.MapFrom(y => y.AirDateUtc))
                .ForMember(x => x.PlexMediaUri, x => x.MapFrom(y => y.PlexEpisode.MediaUri));
            CreateMap<TvRequestSeasonCreateModel, TvRequestSeasonRow>()
                .ForMember(x => x.SeasonIndex, x => x.MapFrom(y => y.Index));
            CreateMap<TvRequestEpisodeCreateModel, TvRequestEpisodeRow>()
                .ForMember(x => x.EpisodeIndex, x => x.MapFrom(y => y.Index));
        }
    }
}