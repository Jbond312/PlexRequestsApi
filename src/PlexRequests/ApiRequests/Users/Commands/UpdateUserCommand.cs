using System;
using System.Collections.Generic;
using MediatR;
using Newtonsoft.Json;
using PlexRequests.Attributes;

namespace PlexRequests.ApiRequests.Users.Commands
{
    public class UpdateUserCommand : IRequest
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public bool IsDisabled { get; set; }
        [ValidRole]
        public List<string> Roles { get; set; }
    }
}
