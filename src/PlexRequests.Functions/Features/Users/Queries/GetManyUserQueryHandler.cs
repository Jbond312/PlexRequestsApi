using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.Functions.Features.Users.Models.Detail;

namespace PlexRequests.Functions.Features.Users.Queries
{
    public class GetManyUserQueryHandler : IRequestHandler<GetManyUserQuery, ValidationContext<GetManyUserQueryResult>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userRepository;

        public GetManyUserQueryHandler(
            IMapper mapper,
            IUserService userRepositoryr)
        {
            _mapper = mapper;
            _userRepository = userRepositoryr;
        }

        public async Task<ValidationContext<GetManyUserQueryResult>> Handle(GetManyUserQuery query, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext<GetManyUserQueryResult>();

            var users = await _userRepository.GetAllUsers(query.IncludeDisabled);

            users = users.Where(x => x.UserId != query.UserInfo.UserId);

            var userModels = _mapper.Map<List<UserDetailModel>>(users);

            resultContext.Data = new GetManyUserQueryResult
            {
                Users = userModels
            };

            return resultContext;
        }
    }
}
