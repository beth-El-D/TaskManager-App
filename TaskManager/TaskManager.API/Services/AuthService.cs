using TaskManager.Infrastructure.Data; // for AppDbContext
using TaskManager.Infrastructure.Security; // for JwtTokenService
using TaskManager.Domain.Entities; // for User
using TaskManager.API.DTOs; // for DTOs
using TaskManager.API.Exceptions; // for custom exceptions
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
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException("Email is required.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ValidationException("Password is required.");

        if (dto.Password.Length < 6)
            throw new ValidationException("Password must be at least 6 characters long.");

        if (!IsValidEmail(dto.Email))
            throw new ValidationException("Please enter a valid email address.");

        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new UserAlreadyExistsException(dto.Email);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<string> Login(LoginDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException("Email is required.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ValidationException("Password is required.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        return _jwt.GenerateToken(user);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

