
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Helpers
{
    public class AgentGuidParserResult
    {
        public AgentGuidParserResult(AgentTypes agentType, string agentSourceId)
        {
            AgentType = agentType;
            AgentSourceId = agentSourceId;
        }

        public AgentTypes AgentType { get; set; }
        public string AgentSourceId { get; set; }
    }
}