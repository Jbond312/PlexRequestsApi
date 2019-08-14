using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Exceptions;
using PlexRequests.Core.Services;
using PlexRequests.Repository.Models;

namespace PlexRequests.ApiRequests.Users.Commands
{
    public class UpdateUserCommandHandler : AsyncRequestHandler<UpdateUserCommand>
    {
        private readonly IUserService _userService;

        public UpdateUserCommandHandler(
            IUserService userService
            )
        {
            _userService = userService;
        }

        protected override async Task Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await ValidateAndReturnUser(command);

            await UpdateUser(command, user);
        }

        private async Task UpdateUser(UpdateUserCommand command, User user)
        {
            user.IsDisabled = command.IsDisabled;

            await _userService.UpdateUser(user);
        }

        private async Task<User> ValidateAndReturnUser(UpdateUserCommand command)
        {
            var user = await _userService.GetUser(command.Id);

            if (user == null)
            {
                throw new PlexRequestException("Invalid user id", "A user for the given id was not fond",
                    HttpStatusCode.NotFound);
            }

            return user;
        }
    }
}
