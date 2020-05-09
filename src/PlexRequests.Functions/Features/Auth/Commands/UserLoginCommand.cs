using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.Functions.Features.Auth.Commands
{
    public class UserLoginCommand : IRequest<ValidationContext<UserLoginCommandResult>>
    {
        [Required]
        [MinLength(1)]
        public string Username { get; set; }
        [Required]
        [MinLength(1)]
        public string Password { get; set; }
    }
}
