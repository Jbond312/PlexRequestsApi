using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.ApiRequests.Issues.DTOs.Detail;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;

namespace PlexRequests.ApiRequests.Issues.Queries
{
    public class GetOneIssueQueryHandler : IRequestHandler<GetOneIssueQuery, GetOneIssueQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IIssueService _issueService;

        public GetOneIssueQueryHandler(
            IMapper mapper,
            IIssueService issueService
        )
        {
            _mapper = mapper;
            _issueService = issueService;
        }

        public async Task<GetOneIssueQueryResult> Handle(GetOneIssueQuery request, CancellationToken cancellationToken)
        {
            var issue = await _issueService.GetIssueById(request.Id);

            if (issue == null)
            {
                throw new PlexRequestException("Invalid Id", "No issue found for the given Id", HttpStatusCode.NotFound);
            }

            var issueModel = _mapper.Map<IssueDetailModel>(issue);

            return new GetOneIssueQueryResult
            {
                Issue = issueModel
            };
        }
    }
}