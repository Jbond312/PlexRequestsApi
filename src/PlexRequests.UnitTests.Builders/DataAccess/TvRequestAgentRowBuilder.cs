using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class TvRequestAgentRowBuilder : IBuilder<TvRequestAgentRow>
    {
        private AgentTypes _agentType;
        private string _agentSourceId;

        public TvRequestAgentRowBuilder()
        {
            _agentType = AgentTypes.TheMovieDb;
            _agentSourceId = "446021";
        }

        public TvRequestAgentRowBuilder WithAgentType(AgentTypes agentType)
        {
            _agentType = agentType;
            return this;
        }

        public TvRequestAgentRowBuilder WithAgentSource(string agentSourceId)
        {
            _agentSourceId = agentSourceId;
            return this;
        }

        public TvRequestAgentRow Build()
        {
            return new TvRequestAgentRow(_agentType, _agentSourceId);
        }
    }
}
