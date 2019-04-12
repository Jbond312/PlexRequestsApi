using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Core;
using PlexRequests.Models.ViewModels;

namespace PlexRequests.Models.Requests
{
    public class GetPagedRequestQueryHandler : IRequestHandler<GetPagedRequestQuery, GetPagedRequestQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsAccessor;

        public GetPagedRequestQueryHandler(IMapper mapper,
            IRequestService requestService, 
            IClaimsPrincipalAccessor claimsAccessor)
        {
            _mapper = mapper;
            _requestService = requestService;
            _claimsAccessor = claimsAccessor;
        }

        public async Task<GetPagedRequestQueryResult> Handle(GetPagedRequestQuery request, CancellationToken cancellationToken)
        {
            Guid? userGuid = null;

            if (request.IncludeCurrentUsersOnly != null && request.IncludeCurrentUsersOnly.Value)
            {
                userGuid = _claimsAccessor.UserId;
            }
            
            var pagedResponse = await _requestService.GetPaged(request.Title, request.MediaType, request.IsApproved,
                userGuid, request.Page, request.PageSize);

            var requestViewModels = _mapper.Map<List<RequestViewModel>>(pagedResponse.Items);

            return new GetPagedRequestQueryResult
            {
                Page = pagedResponse.Page,
                PageSize = pagedResponse.PageSize,
                TotalPages = pagedResponse.TotalPages,
                Items = requestViewModels
            };
        }
    }
}