using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.ApiRequests.Users.Commands
{
    public class UpdateUserCommandHandler : AsyncRequestHandler<UpdateUserCommand>
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserCommandHandler(
            IUserService userService,
            IUnitOfWork unitOfWork
            )
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        protected override async Task Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await ValidateAndReturnUser(command);

            await UpdateUser(command, user);
        }

        private async Task UpdateUser(UpdateUserCommand command, UserRow user)
        {
            user.IsDisabled = command.IsDisabled;
            user.UserRoles = command.Roles.Select(newRole => new UserRoleRow
            {
                Role = newRole
            }).ToList();

            await _unitOfWork.CommitAsync();
        }

        private async Task<UserRow> ValidateAndReturnUser(UpdateUserCommand command)
        {
            var user = await _userService.GetUser(command.Id);

            if (user == null)
            {
                throw new PlexRequestException("Invalid user id", "A user for the given id was not found",
                    HttpStatusCode.NotFound);
            }

            return user;
        }
    }
}
