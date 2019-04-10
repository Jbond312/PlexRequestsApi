using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using PlexRequests.Helpers;
using PlexRequests.Store.Enums;

namespace PlexRequests.Core
{
    public class AgentGuidParser : IAgentGuidParser
    {
        private readonly List<AgentTypes?> _agentTypes;
        private static readonly Regex PlexAgentGuidRegex = new Regex("com\\.plexapp\\.agents\\.(?'agent'.*)://(?'agentId'.*)\\?");

        public AgentGuidParser()
        {
            _agentTypes = Enum.GetValues(typeof(AgentTypes)).Cast<AgentTypes?>().ToList();
        }
        
        public (AgentTypes agentType, string agentSourceId) TryGetAgentDetails(string agentGuid)
        {
            CheckForEmptyGuid(agentGuid);

            var match = GetRegexMatch(agentGuid);

            var agent = match.Groups["agent"].Value;
            var agentId = match.Groups["agentId"].Value;

            var matchingSource = _agentTypes.FirstOrDefault(x =>
                x.ToString().Contains(agent, StringComparison.CurrentCultureIgnoreCase));

            if (matchingSource == null)
            {
                throw CreateException("No AgentType could be extracted from the agent guid",   agentGuid);
            }

            if (matchingSource.Value == AgentTypes.TheTvDb && agentId.Contains("/"))
            {
                agentId = agentId.Split("/")[0];
            }

            return (matchingSource.Value, agentId);
        }

        private static Match GetRegexMatch(string agentGuid)
        {
            var match = PlexAgentGuidRegex.Match(agentGuid);

            if (!match.Success)
            {
                throw CreateException("The PlexMetadataGuid was not in the expected format", agentGuid);
            }

            return match;
        }

        private static void CheckForEmptyGuid(string agentGuid)
        {
            if (!string.IsNullOrEmpty(agentGuid))
            {
                return;
            }

            throw CreateException("The PlexMetadataGuid should not be null or empty");
        }

        private static PlexRequestException CreateException(string description, object logObject = null)
        {
            throw new PlexRequestException("Invalid Plex Metadata Agent Guid", description, HttpStatusCode.InternalServerError, logObject);
        }
    }
}