﻿using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace PlexRequests.TheMovieDb.Models
{
    public class TvSearch
    {
        public string Original_Name { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Vote_Count { get; set; }
        public double Vote_Average { get; set; }
        public string Poster_Path { get; set; }
        public string First_Air_Date { get; set; }
        public double Popularity { get; set; }
        public List<int> Genre_Ids { get; set; }
        public string Original_Language { get; set; }
        public string Backdrop_Path { get; set; }
        public string Overview { get; set; }
        public List<string> Origin_Country { get; set; }
    }
}
