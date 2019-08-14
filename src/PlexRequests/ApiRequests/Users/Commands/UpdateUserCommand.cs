using System;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.ApiRequests.Users.Commands
{
    public class UpdateUserCommand : IRequest
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public bool IsDisabled { get; set; }
    }
}
