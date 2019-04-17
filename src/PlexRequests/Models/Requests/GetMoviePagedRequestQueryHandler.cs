using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Core;
using PlexRequests.Models.SubModels.Detail;
using PlexRequests.Store.Enums;

namespace PlexRequests.Models.Requests
{
    public class GetMoviePagedRequestQueryHandler : IRequestHandler<GetMoviePagedRequestQuery, GetMoviePagedRequestQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsAccessor;

        public GetMoviePagedRequestQueryHandler(
            IMapper mapper,
            IRequestService requestService,
            IClaimsPrincipalAccessor claimsAccessor)
        {
            _mapper = mapper;
            _requestService = requestService;
            _claimsAccessor = claimsAccessor;
        }

        public async Task<GetMoviePagedRequestQueryResult> Handle(GetMoviePagedRequestQuery request, CancellationToken cancellationToken)
        {
            Guid? userGuid = null;

            if (request.IncludeCurrentUsersOnly != null && request.IncludeCurrentUsersOnly.Value)
            {
                userGuid = _claimsAccessor.UserId;
            }
            
            var pagedResponse = await _requestService.GetPaged(request.Title, PlexMediaTypes.Movie, request.Status,
                userGuid, request.Page, request.PageSize);

            var requestViewModels = _mapper.Map<List<MovieRequestDetailModel>>(pagedResponse.Items);

            return new GetMoviePagedRequestQueryResult
            {
                Page = pagedResponse.Page,
                PageSize = pagedResponse.PageSize,
                TotalPages = pagedResponse.TotalPages,
                Items = requestViewModels
            };
        }
    }
}