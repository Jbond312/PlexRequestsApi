using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Auth;
using PlexRequests.Core.Services;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;

namespace PlexRequests.Functions.Features.Plex.Commands
{
    public class SyncUsersCommandHandler : IRequestHandler<SyncUsersCommand, ValidationContext>
    {
        private readonly IPlexApi _plexApi;
        private readonly IUserService _userService;
        private readonly IPlexService _plexService;
        private readonly IUnitOfWork _unitOfWork;

        public SyncUsersCommandHandler(
            IPlexApi plexApi,
            IUserService userService,
            IPlexService plexService,
            IUnitOfWork unitOfWork)
        {
            _plexApi = plexApi;
            _userService = userService;
            _plexService = plexService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ValidationContext> Handle(SyncUsersCommand request, CancellationToken cancellationToken)
        {
            var result = new ValidationContext();

            var server = await _plexService.GetServer();

            if (server == null)
            {
                result.AddError("No admin server found", "Cannot sync users as no admin server has been found");
                return result;
            }

            var remoteFriends = await _plexApi.GetFriends(server.AccessToken);

            var existingFriends = await _userService.GetAllUsers();

            DisableDeletedUsers(existingFriends, remoteFriends);

            await CreateNewUsers(remoteFriends);

            await _unitOfWork.CommitAsync();

            return result;
        }

        private async Task CreateNewUsers(List<Friend> remoteFriends)
        {
            foreach (var friend in remoteFriends)
            {
                if (string.IsNullOrEmpty(friend.Email) || await _userService.UserExists(friend.Email))
                {
                    continue;
                }

                var user = new UserRow
                {
                    Identifier = Guid.NewGuid(),
                    Username = friend.Username,
                    Email = friend.Email,
                    PlexAccountId = Convert.ToInt32(friend.Id),
                    UserRoles = new List<UserRoleRow>
                    {
                        new UserRoleRow
                        {
                            Role = PlexRequestRoles.User
                        },
                        new UserRoleRow
                        {
                            Role = PlexRequestRoles.Commenter
                        }
                    }
                };

                await _userService.AddUser(user);
            }
        }

        private static void DisableDeletedUsers(IEnumerable<UserRow> existingFriends, IReadOnlyCollection<Friend> remoteFriends)
        {
            var deletedFriends = existingFriends
                .Where(ef => !remoteFriends.Select(rf => rf.Email).Contains(ef.Email) && !ef.IsAdmin).ToList();

            foreach (var friend in deletedFriends)
            {
                friend.IsDisabled = true;
            }
        }
    }
}
