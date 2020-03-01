using AutoMapper;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Functions.Features.Issues.Models.Detail;
using PlexRequests.Functions.Features.Issues.Models.ListDetail;

namespace PlexRequests.Functions.Mapping
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