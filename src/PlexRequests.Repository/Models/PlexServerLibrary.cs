namespace PlexRequests.Repository.Models
{
    public class PlexServerLibrary
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsArchived { get; set; }
    }
}
