using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Auth.Commands
{
    public class AddAdminCommand : IRequest<UserLoginCommandResult>
    {
        [Required]
        [MinLength(1)]
        public string Username { get; set; }
        [Required]
        [MinLength(1)]
        public string Password { get; set; }
    }
}
