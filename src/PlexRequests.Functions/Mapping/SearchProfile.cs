using System;
using AutoMapper;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.TheMovieDb.Models;

namespace PlexRequests.Functions.Mapping
{
    public class SearchProfile : Profile
    {
        public SearchProfile()
        {
            CreateMap<MovieSearch, MovieSearchModel>()
                .ForMember(x => x.PosterPath, x => x.MapFrom(y => y.Poster_Path))
                .ForMember(x => x.BackdropPath, x => x.MapFrom(y => y.Backdrop_Path))
                .ForMember(x => x.ReleaseDate,
                    x => x.MapFrom(y =>
                        string.IsNullOrEmpty(y.Release_Date) ? null : (DateTime?)DateTime.Parse(y.Release_Date)));

            CreateMap<MovieDetails, MovieDetailModel>()
                .ForMember(x => x.ImdbId, x => x.MapFrom(y => y.Imdb_Id))
                .ForMember(x => x.PosterPath, x => x.MapFrom(y => y.Poster_Path))
                .ForMember(x => x.BackdropPath, x => x.MapFrom(y => y.Backdrop_Path))
                .ForMember(x => x.ReleaseDate,
                    x => x.MapFrom(y =>
                        string.IsNullOrEmpty(y.Release_Date) ? null : (DateTime?)DateTime.Parse(y.Release_Date)));
        }
    }
}