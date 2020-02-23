using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.UnitTests.Builders.DataAccess
{
    public class MovieRequestAgentRowBuilder : IBuilder<MovieRequestAgentRow>
    {
        private AgentTypes _agentType;
        private string _agentSourceId;

        public MovieRequestAgentRowBuilder()
        {
            _agentType = AgentTypes.TheMovieDb;
            _agentSourceId = "446021";
        }

        public MovieRequestAgentRowBuilder WithAgentType(AgentTypes agentType)
        {
            _agentType = agentType;
            return this;
        }

        public MovieRequestAgentRowBuilder WithAgentSource(string agentSourceId)
        {
            _agentSourceId = agentSourceId;
            return this;
        }

        public MovieRequestAgentRow Build()
        {
            return new MovieRequestAgentRow(_agentType, _agentSourceId);
        }
    }
}
