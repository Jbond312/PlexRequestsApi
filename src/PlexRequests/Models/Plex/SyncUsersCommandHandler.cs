using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Core.Auth;
using PlexRequests.Core.Services;
using PlexRequests.Plex;
using PlexRequests.Plex.Models;
using User = PlexRequests.Repository.Models.User;

namespace PlexRequests.Models.Plex
{
    public class SyncUsersCommandHandler : AsyncRequestHandler<SyncUsersCommand>
    {
        private readonly IPlexApi _plexApi;
        private readonly IUserService _userService;
        private readonly IPlexService _plexService;

        public SyncUsersCommandHandler(
            IPlexApi plexApi,
            IUserService userService,
            IPlexService plexService)
        {
            _plexApi = plexApi;
            _userService = userService;
            _plexService = plexService;
        }

        protected override async Task Handle(SyncUsersCommand request, CancellationToken cancellationToken)
        {
            var server = await _plexService.GetServer();

            var remoteFriends = await _plexApi.GetFriends(server.AccessToken);

            var existingFriends = await _userService.GetAllUsers();

            await DisableDeletedUsers(existingFriends, remoteFriends);

            await CreateNewUsers(remoteFriends);
        }

        private async Task CreateNewUsers(List<Friend> remoteFriends)
        {
            foreach (var friend in remoteFriends)
            {
                if (string.IsNullOrEmpty(friend.Email) || await _userService.UserExists(friend.Email))
                {
                    continue;
                }

                var user = new User
                {
                    Username = friend.Username,
                    Email = friend.Email,
                    PlexAccountId = Convert.ToInt32(friend.Id),
                    Roles = new List<string> { PlexRequestRoles.User }
                };

                await _userService.CreateUser(user);
            }
        }

        private async Task DisableDeletedUsers(List<User> existingFriends, List<Friend> remoteFriends)
        {
            var deletedFriends = existingFriends
                .Where(ef => !remoteFriends.Select(rf => rf.Email).Contains(ef.Email) && !ef.IsAdmin).ToList();

            foreach (var friend in deletedFriends)
            {
                friend.IsDisabled = true;
                await _userService.UpdateUser(friend);
            }
        }
    }
}
