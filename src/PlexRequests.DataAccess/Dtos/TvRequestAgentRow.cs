using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Dtos
{
    [Table("TvRequestAgents", Schema = "Plex")]
    public class TvRequestAgentRow : TimestampRow
    {
        public TvRequestAgentRow()
        {
            
        }

        public TvRequestAgentRow(AgentTypes agentType, string agentSourceId)
        {
            AgentType = agentType;
            AgentSourceId = agentSourceId;
        }

        [Key]
        public int MovieRequestAgentId { get; set; }
        [ForeignKey("TvRequestId")]
        public virtual TvRequestRow TvRequest { get; set; }
        public int MovieRequestId { get; set; }
        [Column("AgentTypeId")]
        public AgentTypes AgentType { get; set; }
        public string AgentSourceId { get; set; }
    }
}
