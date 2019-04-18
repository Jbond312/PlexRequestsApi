using PlexRequests.Repository.Enums;

namespace PlexRequests.Core.Helpers
{
    public interface IAgentGuidParser
    {
        (AgentTypes agentType, string agentSourceId) TryGetAgentDetails(string agentGuid);
    }
}