using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Helpers
{
    public class AgentGuidParser : IAgentGuidParser
    {
        private readonly ILogger<AgentGuidParser> _logger;
        private readonly List<AgentTypes?> _agentTypes;
        private static readonly Regex PlexAgentGuidRegex = new Regex("com\\.plexapp\\.agents\\.(?'agent'.*)://(?'agentId'.*)\\?");

        public AgentGuidParser(
            ILogger<AgentGuidParser> logger
        )
        {
            _logger = logger;
            _agentTypes = Enum.GetValues(typeof(AgentTypes)).Cast<AgentTypes?>().ToList();
        }

        public AgentGuidParserResult TryGetAgentDetails(string agentGuid)
        {
            if (string.IsNullOrWhiteSpace(agentGuid))
            {
                _logger.LogError($"The PlexMetadataGuid is expected but no value was specified");
                return null;
            }

            var match = GetRegexMatch(agentGuid);

            if (!match.Success)
            {
                _logger.LogError($"The PlexMetadataGuid was not in the expected format: {agentGuid}");
                return null;
            }

            var agent = match.Groups["agent"].Value;
            var agentId = match.Groups["agentId"].Value;

            var matchingSource = _agentTypes.FirstOrDefault(x =>
                x.ToString().Contains(agent, StringComparison.CurrentCultureIgnoreCase));

            if (matchingSource == null)
            {
                _logger.LogError($"No AgentType could be extracted from the agent guid: {agentGuid}");
                return null;
            }

            if (matchingSource.Value == AgentTypes.TheTvDb && agentId.Contains("/"))
            {
                agentId = agentId.Split("/")[0];
            }

            return new AgentGuidParserResult(matchingSource.Value, agentId);
        }

        private static Match GetRegexMatch(string agentGuid)
        {
            return PlexAgentGuidRegex.Match(agentGuid);
        }
    }
}