namespace PlexRequests.Core.Helpers
{
    public interface IAgentGuidParser
    {
        AgentGuidParserResult TryGetAgentDetails(string agentGuid);
    }
}