using System.Threading.Tasks;
using PlexRequests.Store;

namespace PlexRequests.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<Store.Models.Settings> Get()
        {
            return await _settingsRepository.GetSettings();
        }

        public async Task Save(Store.Models.Settings settings)
        {
            await _settingsRepository.UpdateSettings(settings);
        }

        public async Task PrimeSettings(Store.Models.Settings settings, bool overwrite)
        {
            await _settingsRepository.PrimeSettings(settings, overwrite);
        }
    }
}
