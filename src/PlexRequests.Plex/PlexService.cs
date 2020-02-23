using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRequests.Core.Exceptions;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;
using PlexRequests.DataAccess.Repositories;

namespace PlexRequests.Plex
{
    public class PlexService : IPlexService
    {
        private readonly IPlexServerRepository _plexServerRepository;
        private readonly IPlexMediaItemRepository _plexMediaRepository;

        public PlexService(
            IPlexServerRepository plexServerRepository,
            IPlexMediaItemRepository plexMediaRepository
            )
        {
            _plexServerRepository = plexServerRepository;
            _plexMediaRepository = plexMediaRepository;
        }

        public async Task<PlexServerRow> GetServer()
        {
            var server = await _plexServerRepository.Get();

            if (server == null)
            {
                throw new PlexRequestException("Unable to find the Admin server");
            }

            return server;
        }

        public async Task AddServer(PlexServerRow server)
        {
            await _plexServerRepository.Add(server);
        }

        public async Task<List<PlexMediaItemRow>> GetMediaItems(PlexMediaTypes mediaType)
        {
            return await _plexMediaRepository.GetMany(x => x.MediaType == mediaType);
        }

        public async Task<PlexMediaItemRow> GetExistingMediaItemByAgent(PlexMediaTypes mediaType, AgentTypes agentType, string agentSourceId)
        {
            return await _plexMediaRepository.GetOne(x =>
                x.MediaType == mediaType &&
                x.AgentType == agentType &&
                x.AgentSourceId == agentSourceId);
        }

        public async Task CreateMany(List<PlexMediaItemRow> mediaItems)
        {
            if (!mediaItems.Any())
            {
                return;
            }
            await _plexMediaRepository.CreateMany(mediaItems);
        }

        public void DeleteAllMediaItems()
        {
            _plexMediaRepository.DeleteAll();
        }
    }
}
