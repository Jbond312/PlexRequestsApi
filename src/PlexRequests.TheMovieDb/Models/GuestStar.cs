// ReSharper disable InconsistentNaming
namespace PlexRequests.TheMovieDb.Models
{
    public class GuestStar
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Credit_Id { get; set; }
        public string Character { get; set; }
        public int Order { get; set; }
        public int Gender { get; set; }
        public string Profile_Path { get; set; }
    }
}
