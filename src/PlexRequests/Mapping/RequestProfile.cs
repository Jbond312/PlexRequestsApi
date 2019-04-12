using AutoMapper;
using PlexRequests.Models.ViewModels;
using PlexRequests.Store.Models;

namespace PlexRequests.Mapping
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {
            CreateMap<RequestViewModel, Request>();
            CreateMap<RequestSeasonViewModel, RequestSeason>();
            CreateMap<RequestEpisodeViewModel, RequestEpisode>();
        }
    }
}