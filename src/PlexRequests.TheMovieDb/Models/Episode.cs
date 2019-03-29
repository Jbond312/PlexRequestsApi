using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace PlexRequests.TheMovieDb.Models
{
    public class Episode
    {
        public string Air_Date { get; set; }
        public int Episode_Number { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public string Production_Code { get; set; }
        public int Season_Number { get; set; }
        public int Show_Id { get; set; }
        public string Still_Path { get; set; }
        public double Vote_Average { get; set; }
        public int Vote_Count { get; set; }
        public List<Crew> Crew { get; set; }
        public List<GuestStar> Guest_Stars { get; set; }
    }
}
