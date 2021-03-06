using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess.Enums;
using PlexRequests.Functions.Features.Issues.Models.ListDetail;

namespace PlexRequests.Functions.Features.Issues.Queries
{
    public class GetPagedIssueQueryHandler : IRequestHandler<GetPagedIssueQuery, ValidationContext<GetPagedIssueQueryResult>>
    {
        private readonly IMapper _mapper;
        private readonly IIssueService _issueService;

        public GetPagedIssueQueryHandler(
            IMapper mapper,
            IIssueService issueService
        )
        {
            _mapper = mapper;
            _issueService = issueService;
        }

        public async Task<ValidationContext<GetPagedIssueQueryResult>> Handle(GetPagedIssueQuery request, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext<GetPagedIssueQueryResult>();

            var includedStatuses = new List<IssueStatuses>{
                IssueStatuses.InProgress,
                IssueStatuses.Pending
            };

            if (request.IncludeResolved)
            {
                includedStatuses.Add(IssueStatuses.Resolved);
            }

            var pagedResponse = await _issueService.GetPaged(request.Page, request.PageSize, includedStatuses);

            var issueViewModels = _mapper.Map<List<IssueListDetailModel>>(pagedResponse.Items);

            resultContext.Data = new GetPagedIssueQueryResult
            {
                Page = pagedResponse.Page,
                PageSize = pagedResponse.PageSize,
                TotalPages = pagedResponse.TotalPages,
                Items = issueViewModels
            };

            return resultContext;
        }
    }
}