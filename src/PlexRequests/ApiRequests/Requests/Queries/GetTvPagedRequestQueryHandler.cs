using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.ApiRequests.Requests.Models.Detail;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetTvPagedRequestQueryHandler : IRequestHandler<GetTvPagedRequestQuery, GetTvPagedRequestQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly ITvRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsAccessor;

        public GetTvPagedRequestQueryHandler(IMapper mapper,
            ITvRequestService requestService, 
            IClaimsPrincipalAccessor claimsAccessor)
        {
            _mapper = mapper;
            _requestService = requestService;
            _claimsAccessor = claimsAccessor;
        }

        public async Task<GetTvPagedRequestQueryResult> Handle(GetTvPagedRequestQuery request, CancellationToken cancellationToken)
        {
            int? userId = null;

            if (request.IncludeCurrentUsersOnly != null && request.IncludeCurrentUsersOnly.Value)
            {
                userId = _claimsAccessor.UserId;
            }
            
            var pagedResponse = await _requestService.GetPaged(request.Title, request.Status, userId, request.Page, request.PageSize);

            var requestViewModels = _mapper.Map<List<TvRequestDetailModel>>(pagedResponse.Items);

            return new GetTvPagedRequestQueryResult
            {
                Page = pagedResponse.Page,
                PageSize = pagedResponse.PageSize,
                TotalPages = pagedResponse.TotalPages,
                Items = requestViewModels
            };
        }
    }
}