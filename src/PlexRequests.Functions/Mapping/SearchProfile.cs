using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PlexRequests.Functions.Features.Search.Models;
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

            CreateMap<TvSearch, TvSearchModel>()
                .ForMember(x => x.Title, x => x.MapFrom(y => y.Name))
                .ForMember(x => x.PosterPath, x => x.MapFrom(y => y.Poster_Path))
                .ForMember(x => x.BackdropPath, x => x.MapFrom(y => y.Backdrop_Path))
                .ForMember(x => x.ReleaseDate,
                    x => x.MapFrom(y =>
                        string.IsNullOrEmpty(y.First_Air_Date) ? null : (DateTime?)DateTime.Parse(y.First_Air_Date)));

            CreateMap<TvDetails, TvDetailModel>()
                .ForMember(x => x.Title, x => x.MapFrom(y => y.Name))
                .ForMember(x => x.PosterPath, x => x.MapFrom(y => y.Poster_Path))
                .ForMember(x => x.BackdropPath, x => x.MapFrom(y => y.Backdrop_Path))
                .ForMember(x => x.ReleaseDate,
                    x => x.MapFrom(y =>
                        string.IsNullOrEmpty(y.First_Air_Date) ? null : (DateTime?)DateTime.Parse(y.First_Air_Date)))
                .ForMember(x => x.EpisodeCount, x => x.MapFrom(y => y.Number_Of_Episodes))
                .ForMember(x => x.SeasonCount, x => x.MapFrom(y => y.Number_Of_Seasons))
                .ForMember(x => x.InProduction, x => x.MapFrom(y => y.In_Production))
                .ForMember(x => x.NextEpisode, x => x.MapFrom(y => y.Next_Episode_To_Air))
                .ForMember(x => x.LastEpisode, x => x.MapFrom(y => y.Last_Episode_To_Air))
                .ForMember(x => x.Networks,
                    x => x.MapFrom(y =>
                        y.Networks == null ? new List<string>() : y.Networks.Select(n => n.Name).ToList()));

            CreateMap<TvSeasonDetails, TvSeasonDetailModel>()
                .ForMember(x => x.Id, x => x.MapFrom(y => y._id))
                .ForMember(x => x.Index, x => x.MapFrom(y => y.Season_Number))
                .ForMember(x => x.PosterPath, x => x.MapFrom(y => y.Poster_Path))
                .ForMember(x => x.AirDate,
                    x => x.MapFrom(
                        y => string.IsNullOrEmpty(y.Air_Date) ? null : (DateTime?)DateTime.Parse(y.Air_Date)));

            CreateMap<EpisodeToAir, EpisodeToAirModel>()
                .ForMember(x => x.AirDate,
                    x => x.MapFrom(
                        y => string.IsNullOrEmpty(y.Air_Date) ? null : (DateTime?)DateTime.Parse(y.Air_Date)))
                .ForMember(x => x.Season, x => x.MapFrom(y => y.Season_Number))
                .ForMember(x => x.Episode, x => x.MapFrom(y => y.Episode_Number))
                .ForMember(x => x.StillPath, x => x.MapFrom(y => y.Still_Path));

            CreateMap<Episode, EpisodeModel>()
                .ForMember(x => x.Index, x => x.MapFrom(y => y.Episode_Number))
                .ForMember(x => x.SeasonIndex, x => x.MapFrom(y => y.Season_Number))
                .ForMember(x => x.StillPath, x => x.MapFrom(y => y.Still_Path));
        }
    }
}