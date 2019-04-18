using System.Threading.Tasks;

namespace PlexRequests.Core.Settings
{
    public interface ISettingsService
    {
        Task<Repository.Models.Settings> Get();
        Task Save(Repository.Models.Settings settings);
        Task PrimeSettings(Repository.Models.Settings settings);
    }
}
