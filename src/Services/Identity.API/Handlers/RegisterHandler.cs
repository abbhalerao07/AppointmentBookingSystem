using Identity.API.Commands;
using Identity.API.Data;
using Identity.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Handlers
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly IdentityDbContext _context;

        public RegisterHandler(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if user exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
                throw new InvalidOperationException($"User with email {request.Email} already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return new RegisterResponse(user.Id, user.Email, "User registered successfully");
        }
    }
}
