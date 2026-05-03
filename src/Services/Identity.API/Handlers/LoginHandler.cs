using Identity.API.Commands;
using Identity.API.Data;
using Identity.API.Models;
using Identity.API.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Handlers
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IdentityDbContext _context;
        private readonly ITokenService _tokenService;

        public LoginHandler(IdentityDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Store refresh token
            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return new LoginResponse(
                accessToken,
                refreshToken,
                DateTime.UtcNow.AddMinutes(15),
                new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString())
            );
        }
    }
}
