using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.Functions.Features.Issues.Models.Detail;

namespace PlexRequests.Functions.Features.Issues.Queries
{
    public class GetOneIssueQueryHandler : IRequestHandler<GetOneIssueQuery, ValidationContext<GetOneIssueQueryResult>>
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

        public async Task<ValidationContext<GetOneIssueQueryResult>> Handle(GetOneIssueQuery request, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext<GetOneIssueQueryResult>();

            var issue = await _issueService.GetIssueById(request.Id);

            resultContext.AddErrorIf(() => issue == null, "Invalid Id", "No issue found for the given id");

            if (!resultContext.IsSuccessful)
            {
                return resultContext;
            }

            var issueModel = _mapper.Map<IssueDetailModel>(issue);

            resultContext.Data = new GetOneIssueQueryResult
            {
                Issue = issueModel
            };

            return resultContext;
        }
    }
}