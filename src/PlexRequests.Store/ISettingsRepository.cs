using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public interface ISettingsRepository
    {
        Task<Settings> GetSettings();
        Task UpdateSettings(Settings settings);
        Task PrimeSettings(Settings settings, bool overwrite);
    }
}
