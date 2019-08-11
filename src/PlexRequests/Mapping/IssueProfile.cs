using AutoMapper;
using PlexRequests.ApiRequests.Requests.DTOs;
using PlexRequests.Repository.Models;

namespace PlexRequests.Mapping
{
    public class IssueProfile : Profile
    {
        public IssueProfile()
        {
            CreateMap<Issue, IssueDetailModel>();
            CreateMap<Issue, IssueListDetailModel>();
            CreateMap<IssueComment, IssueCommentDetailModel>();
        }
    }
}