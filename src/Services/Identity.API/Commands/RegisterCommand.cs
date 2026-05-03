using Common.CQRS;
using Identity.API.Models;

namespace Identity.API.Commands
{
    public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    UserRole Role
) : ICommand<RegisterResponse>;

    public record RegisterResponse(Guid UserId, string Email, string Message);
}
