using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Core;
using PlexRequests.Core.Exceptions;
using PlexRequests.Repository;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Plex
{
    public class PlexService : IPlexService
    {
        private readonly IPlexServerRepository _plexServerRepository;
        private readonly IPlexMediaRepository _plexMediaRepository;

        public PlexService(
            IPlexServerRepository plexServerRepository,
            IPlexMediaRepository plexMediaRepository
            )
        {
            _plexServerRepository = plexServerRepository;
            _plexMediaRepository = plexMediaRepository;
        }

        public async Task<PlexServer> GetServer()
        {
            var server = await _plexServerRepository.Get();

            if (server == null)
            {
                throw new PlexRequestException("Unable to find the Admin server");
            }

            return server;
        }

        public async Task<PlexServer> Create(PlexServer server)
        {
            return await _plexServerRepository.Create(server);
        }

        public async Task Update(PlexServer server)
        {
            await _plexServerRepository.Update(server);
        }

        public async Task<List<PlexMediaItem>> GetMediaItems(PlexMediaTypes mediaType)
        {
            return await _plexMediaRepository.GetMany(x => x.MediaType == mediaType);
        }

        public async Task<PlexMediaItem> GetExistingMediaItemByAgent(PlexMediaTypes mediaType, AgentTypes agentType, string agentSourceId)
        {
            return await _plexMediaRepository.GetOne(x =>
                x.MediaType == mediaType &&
                x.AgentType == agentType &&
                x.AgentSourceId == agentSourceId);
        }

        public async Task CreateMany(List<PlexMediaItem> mediaItems)
        {
            if (!mediaItems.Any())
            {
                return;
            }
            await _plexMediaRepository.CreateMany(mediaItems);
        }

        public async Task UpdateMany(List<PlexMediaItem> mediaItems)
        {
            foreach (var mediaItem in mediaItems)
            {
                await _plexMediaRepository.Update(mediaItem);
            }
        }

        public async Task DeleteAllMediaItems()
        {
            await _plexMediaRepository.DeleteAll();
        }
    }
}
