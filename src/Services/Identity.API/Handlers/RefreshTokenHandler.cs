using Identity.API.Commands;
using Identity.API.Data;
using Identity.API.Models;
using Identity.API.Services;
using MediatR;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Handlers
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
    {
        private readonly IdentityDbContext _context;
        private readonly ITokenService _tokenService;

        public RefreshTokenHandler(IdentityDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
                throw new UnauthorizedAccessException("Invalid access token");

            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId, cancellationToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revoke old token
            storedToken.IsRevoked = true;

            // Store new token
            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return new LoginResponse(
                newAccessToken,
                newRefreshToken,
                DateTime.UtcNow.AddMinutes(15),
                new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString())
            );
        }
    }
}
