using System.Threading.Tasks;

namespace PlexRequests.Settings
{
    public interface ISettingsService
    {
        Task<Store.Models.Settings> Get();
        Task Save(Store.Models.Settings settings);
        Task PrimeSettings(Store.Models.Settings settings);
    }
}
