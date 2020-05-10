using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Core.Helpers;
using PlexRequests.Core.Services;
using PlexRequests.Functions.Features.Users.Models.Detail;

namespace PlexRequests.Functions.Features.Users.Queries
{
    public class GetManyUserQueryHandler : IRequestHandler<GetManyUserQuery, ValidationContext<GetManyUserQueryResult>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userRepository;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public GetManyUserQueryHandler(
            IMapper mapper,
            IUserService userRepository,
            IClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
        }

        public async Task<ValidationContext<GetManyUserQueryResult>> Handle(GetManyUserQuery query, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext<GetManyUserQueryResult>();

            var users = await _userRepository.GetAllUsers(query.IncludeDisabled);

            users = users.Where(x => x.UserId != _claimsPrincipalAccessor.UserId);

            var userModels = _mapper.Map<List<UserDetailModel>>(users);

            resultContext.Data = new GetManyUserQueryResult
            {
                Users = userModels
            };

            return resultContext;
        }
    }
}
