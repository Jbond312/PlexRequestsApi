using System;

namespace PlexRequests.DataAccess.Dtos
{
    public class TimestampRow
    {
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }
    }
}
