using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.ApiRequests.Issues.Models.ListDetail;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Issues.Queries
{
    public class GetPagedIssueQueryHandler : IRequestHandler<GetPagedIssueQuery, GetPagedIssueQueryResult>
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

        public async Task<GetPagedIssueQueryResult> Handle(GetPagedIssueQuery request, CancellationToken cancellationToken)
        {
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

            return new GetPagedIssueQueryResult
            {
                Page = pagedResponse.Page,
                PageSize = pagedResponse.PageSize,
                TotalPages = pagedResponse.TotalPages,
                Items = issueViewModels
            };
        }
    }
}