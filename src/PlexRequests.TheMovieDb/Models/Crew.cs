// ReSharper disable InconsistentNaming
namespace PlexRequests.TheMovieDb.Models
{
    public class Crew
    {
        public int Id { get; set; }
        public string Credit_Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Job { get; set; }
        public int Gender { get; set; }
        public string Profile_Path { get; set; }
    }
}
