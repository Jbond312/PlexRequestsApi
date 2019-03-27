using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlexRequests.Attributes;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models;
using PlexRequests.Plex;
using PlexRequests.Store.Models;

namespace PlexRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlexController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IPlexApi _plexApi;
        private readonly IUserService _userService;
        private readonly IPlexService _plexService;

        public PlexController(
            IMapper mapper,
            IPlexApi plexApi, 
            IUserService userService,
            IPlexService plexService)
        {
            _mapper = mapper;
            _plexApi = plexApi;
            _userService = userService;
            _plexService = plexService;
        }

        [HttpPost]
        [Route("ImportUsers")]
        [Admin]
        public async Task ImportUsers(ImportUserRequest importUserRequest)
        {
            var friends = await _plexApi.GetFriends(importUserRequest.PlexToken);

            foreach (var friend in friends)
            {
                if (string.IsNullOrEmpty(friend.Email))
                {
                    continue;
                }

                var user = new User
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

        [HttpGet]
        [Route("Servers")]
        [Admin]
        public async Task<List<PlexServerModel>> GetServers()
        {
            var servers = await _plexService.GetServers();
            return _mapper.Map<List<PlexServerModel>>(servers);
        }
    }
}