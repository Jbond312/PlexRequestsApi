using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("MovieRequestAgents", Schema = "Plex")]
    public class MovieRequestAgentRow : TimestampRow
    {
        public MovieRequestAgentRow()
        {
            
        }

        public MovieRequestAgentRow(AgentTypes agentType, string agentSourceId)
        {
            AgentType = agentType;
            AgentSourceId = agentSourceId;
        }

        [Key]
        public int MovieRequestAgentId { get; set; }
        [ForeignKey("MovieRequestId")]
        public virtual MovieRequestRow MovieRequest { get; set; }
        public int MovieRequestId { get; set; }
        [Column("AgentTypeId")]
        public AgentTypes AgentType { get; set; }
        public string AgentSourceId { get; set; }
    }
}
