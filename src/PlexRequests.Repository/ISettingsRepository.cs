using System.Threading.Tasks;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public interface ISettingsRepository
    {
        Task<Settings> GetSettings();
        Task UpdateSettings(Settings settings);
        Task PrimeSettings(Settings settings, bool overwrite);
    }
}
