using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.ApiRequests.Requests.DTOs.Detail;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetTvPagedRequestQueryHandler : IRequestHandler<GetTvPagedRequestQuery, GetTvPagedRequestQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsAccessor;

        public GetTvPagedRequestQueryHandler(IMapper mapper,
            IRequestService requestService, 
            IClaimsPrincipalAccessor claimsAccessor)
        {
            _mapper = mapper;
            _requestService = requestService;
            _claimsAccessor = claimsAccessor;
        }

        public async Task<GetTvPagedRequestQueryResult> Handle(GetTvPagedRequestQuery request, CancellationToken cancellationToken)
        {
            Guid? userGuid = null;

            if (request.IncludeCurrentUsersOnly != null && request.IncludeCurrentUsersOnly.Value)
            {
                userGuid = _claimsAccessor.UserId;
            }
            
            var pagedResponse = await _requestService.GetPaged(request.Title, PlexMediaTypes.Show, request.Status,
                userGuid, request.Page, request.PageSize);

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