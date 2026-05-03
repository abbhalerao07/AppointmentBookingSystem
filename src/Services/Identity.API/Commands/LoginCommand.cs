using Common.CQRS;

namespace Identity.API.Commands
{
    public record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

    public record LoginResponse(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAt,
        UserDto User
    );

    public record UserDto(Guid Id, string Email, string FirstName, string LastName, string Role);
}
