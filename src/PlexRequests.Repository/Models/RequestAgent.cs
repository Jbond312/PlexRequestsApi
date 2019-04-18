using PlexRequests.Repository.Enums;

namespace PlexRequests.Repository.Models
{
    public class RequestAgent
    {
        public AgentTypes AgentType { get; }
        public string AgentSourceId { get; }

        public RequestAgent(AgentTypes agentType, string agentSourceId)
        {
            AgentType = agentType;
            AgentSourceId = agentSourceId;
        }

        protected bool Equals(RequestAgent other)
        {
            return AgentType == other.AgentType && string.Equals(AgentSourceId, other.AgentSourceId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RequestAgent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) AgentType * 397) ^ (AgentSourceId != null ? AgentSourceId.GetHashCode() : 0);
            }
        }
    }
}