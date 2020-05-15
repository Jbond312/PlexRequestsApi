using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.Functions.Features.Requests.Models.Detail;

namespace PlexRequests.Functions.Features.Requests.Queries
{
    public class GetMoviePagedRequestQueryHandler : IRequestHandler<GetMoviePagedRequestQuery, GetMoviePagedRequestQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IMovieRequestService _requestService;

        public GetMoviePagedRequestQueryHandler(
            IMapper mapper,
            IMovieRequestService requestService)
        {
            _mapper = mapper;
            _requestService = requestService;
        }

        public async Task<GetMoviePagedRequestQueryResult> Handle(GetMoviePagedRequestQuery request, CancellationToken cancellationToken)
        {
            int? userId = null;

            if (request.IncludeCurrentUsersOnly != null && request.IncludeCurrentUsersOnly.Value)
            {
                userId = request.UserInfo.UserId;
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