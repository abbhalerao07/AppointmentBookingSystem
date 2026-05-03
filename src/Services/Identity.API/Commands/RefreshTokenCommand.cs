using Common.CQRS;

namespace Identity.API.Commands
{
    public record RefreshTokenCommand(string AccessToken, string RefreshToken)
    : ICommand<LoginResponse>;
}
