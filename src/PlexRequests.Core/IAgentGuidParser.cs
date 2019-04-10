using PlexRequests.Store.Enums;

namespace PlexRequests.Core
{
    public interface IAgentGuidParser
    {
        (AgentTypes agentType, string agentSourceId) TryGetAgentDetails(string agentGuid);
    }
}