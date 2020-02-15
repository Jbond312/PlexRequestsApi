using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("PlexLibraries", Schema = "Plex")]
    public class PlexLibraryRow : TimestampRow
    {
        public PlexLibraryRow()
        {
            PlexMediaItems = new List<PlexMediaItemRow>();
        }

        [Key]
        public int PlexLibraryId { get; set; }
        [ForeignKey("PlexServerId")]
        public virtual PlexServerRow PlexServer { get; set; }
        public int PlexServerId { get; set; }
        public string LibraryKey { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsArchived { get; set; }
        public virtual ICollection<PlexMediaItemRow> PlexMediaItems { get; set; }
    }
}
