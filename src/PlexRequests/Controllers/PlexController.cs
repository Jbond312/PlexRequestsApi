using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models;
using PlexRequests.Plex;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlexController : Controller
    {
        private readonly IPlexApi _plexApi;
        private readonly IUserService _userService;

        public PlexController(IPlexApi plexApi, IUserService userService)
        {
            _plexApi = plexApi;
            _userService = userService;
        }

        [HttpPost]
        [Route("ImportUsers")]
        [Authorize(Roles = "Admin")]
        public async Task ImportUsers(ImportUserRequest importUserRequest)
        {
            var friends = await _plexApi.GetFriends(importUserRequest.PlexToken);

            foreach (var friend in friends)
            {
                if (string.IsNullOrEmpty(friend.Email))
                {
                    continue;
                }

                var user = new Store.Models.User
                {
                    Username = friend.Username,
                    Email = friend.Email,
                    PlexAccountId = Convert.ToInt32(friend.Id),
                    Roles = new List<string> {PlexRequestRoles.User}
                };

                if (!await _userService.UserExists(user.Email))
                {
                    await _userService.CreateUser(user);
                }
            }
        }
    }
}