using TaskManager.Infrastructure.Data; // for AppDbContext
using TaskManager.Infrastructure.Security; // for JwtTokenService
using TaskManager.Domain.Entities; // for User
using TaskManager.API.DTOs; // for DTOs
using Microsoft.EntityFrameworkCore; // for async EF methods

namespace TaskManager.API.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly JwtTokenService _jwt;

    public AuthService(AppDbContext context, JwtTokenService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    public async Task Register(RegisterDto dto)
    {
        if (_context.Users.Any(u => u.Email == dto.Email))
            throw new Exception("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<string> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        return _jwt.GenerateToken(user);
    }
}

