using AutoMapper;
using PlexRequests.ApiRequests.Issues.Models.Detail;
using PlexRequests.ApiRequests.Issues.Models.ListDetail;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Mapping
{
    public class IssueProfile : Profile
    {
        public IssueProfile()
        {
            CreateMap<IssueRow, IssueDetailModel>()
                .ForMember(x => x.Id, x => x.MapFrom(y => y.IssueId))
                .ForMember(x => x.MediaItemName, x => x.MapFrom(y => y.PlexMediaItem.Title))
                .ForMember(x => x.MediaType, x => x.MapFrom(y => y.PlexMediaItem.MediaType));
            CreateMap<IssueRow, IssueListDetailModel>()
                .ForMember(x => x.Id, x => x.MapFrom(y => y.IssueId))
                .ForMember(x => x.MediaItemName, x => x.MapFrom(y => y.PlexMediaItem.Title))
                .ForMember(x => x.MediaType, x => x.MapFrom(y => y.PlexMediaItem.MediaType));
            CreateMap<IssueCommentRow, IssueCommentDetailModel>()
                .ForMember(x => x.UserName, x => x.MapFrom(y => y.User.Username));
        }
    }
}