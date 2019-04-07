using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Helpers;
using PlexRequests.Store;
using PlexRequests.Store.Models;

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

        public async Task<List<PlexMediaItem>> GetMediaItems(Expression<Func<PlexMediaItem, bool>> filter = null)
        {
            return await _plexMediaRepository.GetMany(filter);
        }

        public async Task<PlexMediaItem> GetOneMediaItem(Expression<Func<PlexMediaItem, bool>> filter = null)
        {
            return await _plexMediaRepository.GetOne(filter);
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
