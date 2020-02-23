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
    public class GetMoviePagedRequestQueryHandler : IRequestHandler<GetMoviePagedRequestQuery, GetMoviePagedRequestQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IMovieRequestService _requestService;
        private readonly IClaimsPrincipalAccessor _claimsAccessor;

        public GetMoviePagedRequestQueryHandler(
            IMapper mapper,
            IMovieRequestService requestService,
            IClaimsPrincipalAccessor claimsAccessor)
        {
            _mapper = mapper;
            _requestService = requestService;
            _claimsAccessor = claimsAccessor;
        }

        public async Task<GetMoviePagedRequestQueryResult> Handle(GetMoviePagedRequestQuery request, CancellationToken cancellationToken)
        {
            int? userId = null;

            if (request.IncludeCurrentUsersOnly != null && request.IncludeCurrentUsersOnly.Value)
            {
                userId = _claimsAccessor.UserId;
            }
            
            var pagedResponse = await _requestService.GetPaged(request.Title, request.Status, userId, request.Page, request.PageSize);

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