using System.Collections.Generic;
using MediatR;
using Newtonsoft.Json;
using PlexRequests.Functions.Attributes;

namespace PlexRequests.Functions.Features.Users.Commands
{
    public class UpdateUserCommand : IRequest<ValidationContext>
    {
        [JsonIgnore]
        public int Id { get; set; }
        public bool IsDisabled { get; set; }
        [ValidRole]
        public List<string> Roles { get; set; }
    }
}
