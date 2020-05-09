using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.Functions.Features.Users.Models.Detail;

namespace PlexRequests.Functions.Features.Users.Queries
{
    public class GetManyUserQueryHandler : IRequestHandler<GetManyUserQuery, GetManyUserQueryResult>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userRepository;

        public GetManyUserQueryHandler(
            IMapper mapper,
            IUserService userRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<GetManyUserQueryResult> Handle(GetManyUserQuery query, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllUsers(query.IncludeDisabled);

            var userModels = _mapper.Map<List<UserDetailModel>>(users);

            return new GetManyUserQueryResult
            {
                Users = userModels
            };
        }
    }
}
