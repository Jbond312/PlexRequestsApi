using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("PlexServers", Schema = "Plex")]
    public class PlexServerRow : TimestampRow
    {
        public PlexServerRow()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            PlexLibraries = new List<PlexLibraryRow>();
        }

        [Key]
        public int PlexServerId { get; set; }
        public Guid Identifier { get; set; }
        public string Name { get; set; }
        public string AccessToken { get; set; }
        public string MachineIdentifier { get; set; }
        public string Scheme { get; set; }
        public string LocalIp { get; set; }
        public int LocalPort { get; set; }
        public string ExternalIp { get; set; }
        public int ExternalPort { get; set; }
        public virtual ICollection<PlexLibraryRow> PlexLibraries { get; set; }

    }
}
