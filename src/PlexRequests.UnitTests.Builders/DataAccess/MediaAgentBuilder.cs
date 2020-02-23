using System;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class MediaAgentBuilder : IBuilder<MediaAgent>
    {
        private AgentTypes _agentType;
        private string _agentSourceId;

        public MediaAgentBuilder()
        {
            _agentType = AgentTypes.TheMovieDb;
            _agentSourceId = Guid.NewGuid().ToString();
        }

        public MediaAgentBuilder WithAgentType(AgentTypes agentType)
        {
            _agentType = agentType;
            return this;
        }

        public MediaAgentBuilder WithAgentSource(string agentSourceId)
        {
            _agentSourceId = agentSourceId;
            return this;
        }

        public MediaAgent Build()
        {
            return new MediaAgent(_agentType, _agentSourceId);
        }
    }
}
