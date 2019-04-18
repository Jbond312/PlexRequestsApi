using System.Threading.Tasks;
using PlexRequests.Core.Services;
using PlexRequests.Repository;

namespace PlexRequests.Core.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly ICacheService _cacheService;
        private const string CacheKey = "Settings";

        public SettingsService(ISettingsRepository settingsRepository, ICacheService cacheService)
        {
            _settingsRepository = settingsRepository;
            _cacheService = cacheService;
        }

        public async Task<Repository.Models.Settings> Get()
        {
            return await _cacheService.GetOrCreate(CacheKey,
                createFunc: async () => await _settingsRepository.GetSettings());

        }

        public async Task Save(Repository.Models.Settings settings)
        {
            await _cacheService.GetOrCreate(CacheKey, async () =>
            {
                await _settingsRepository.UpdateSettings(settings);
                return await _settingsRepository.GetSettings();
            });
        }

        public async Task PrimeSettings(Repository.Models.Settings settings)
        {
            await _cacheService.GetOrCreate(CacheKey, async () =>
            {
                await _settingsRepository.PrimeSettings(settings, settings.OverwriteSettings);
                return await _settingsRepository.GetSettings();
            });  
        }
    }
}
