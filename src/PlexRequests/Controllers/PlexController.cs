using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlexRequests.Attributes;
using PlexRequests.Core;
using PlexRequests.Helpers;
using PlexRequests.Models;
using PlexRequests.Plex;
using PlexRequests.Settings;
using PlexRequests.Store.Models;
using PlexRequests.Sync;
using User = PlexRequests.Store.Models.User;

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
        private readonly IPlexSync _plexSync;
        private readonly PlexSettings _plexSettings;

        public PlexController(
            IMapper mapper,
            IPlexApi plexApi, 
            IUserService userService,
            IPlexService plexService,
            IPlexSync plexSync,
            IOptions<PlexSettings> plexSettings)
        {
            _mapper = mapper;
            _plexApi = plexApi;
            _userService = userService;
            _plexService = plexService;
            _plexSync = plexSync;
            _plexSettings = plexSettings.Value;
        }

        [HttpPost("SyncUsers")]
        [Admin]
        public async Task SyncUsers()
        {
            var server = await _plexService.GetServer();

            var remoteFriends = await _plexApi.GetFriends(server.AccessToken);

            var existingFriends = await _userService.GetAllUsers();

            var deletedFriends = existingFriends.Where(ef => remoteFriends.Select(rf => rf.Email).Contains(ef.Email)).ToList();

            foreach (var friend in deletedFriends)
            {
                friend.IsDisabled = true;
            }

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
                    Roles = new List<string> {PlexRequestRoles.User}
                };

                await _userService.CreateUser(user);
            }
        }

        [HttpPost]
        [Route("SyncLibraries")]
        [Admin]
        public async Task<List<PlexServerLibraryModel>> SyncLibraries()
        {
            var server = await _plexService.GetServer();

            var libraryContainer = await _plexApi.GetLibraries(server.AccessToken, server.GetPlexUri(_plexSettings.ConnectLocally));

            foreach (var existingLibrary in server.Libraries.Where(x => !x.IsArchived))
            {
                var libraryExists = libraryContainer.MediaContainer.Directory.Any(x => x.Key == existingLibrary.Key);

                if (libraryExists)
                {
                    continue;
                }

                existingLibrary.IsArchived = true;
                existingLibrary.IsEnabled = false;
            }

            var newLibraries =
                libraryContainer.MediaContainer.Directory.Where(rl =>
                    !server.Libraries.Select(x => x.Key).Contains(rl.Key)).Select(nl => new PlexServerLibrary
                {
                    Key = nl.Key,
                    Title = nl.Title,
                    Type = nl.Type,
                    IsEnabled = false
                });

            server.Libraries.AddRange(newLibraries);

            await _plexService.Update(server);

            return _mapper.Map<List<PlexServerLibraryModel>>(server.Libraries);
        }

        [HttpPost]
        [Route("SyncContent")]
        [Admin]
        public async Task SyncContent()
        {
            await _plexSync.Synchronise();
        }

        [HttpGet]
        [Route("Server")]
        [Admin]
        public async Task<PlexServerModel> GetServer()
        {
            var server = await _plexService.GetServer();
            return _mapper.Map<PlexServerModel>(server);
        }
    }
}