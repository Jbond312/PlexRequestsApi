using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.ApiRequests.Users.Commands
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ValidationContext>
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

        public async Task<ValidationContext> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var resultContext = new ValidationContext();

            var user = await ValidateAndReturnUser(command, resultContext);

            if (!resultContext.IsSuccessful)
            {
                return resultContext;
            }

            await UpdateUser(command, user);

            return resultContext;
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

        private async Task<UserRow> ValidateAndReturnUser(UpdateUserCommand command, ValidationContext resultContext)
        {
            var user = await _userService.GetUser(command.Id);

            if (user == null)
            {
                resultContext.AddError("Invalid user id", "A user for the given id was not found");
            }

            return user;
        }
    }
}
