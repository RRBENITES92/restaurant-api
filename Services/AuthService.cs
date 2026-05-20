using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RestaurantApi.Data;
using RestaurantApi.DTOs;
using RestaurantApi.Models;

namespace RestaurantApi.Services.Auth;

public class AuthService : IAuthService
{
    private readonly RestaurantDbContext _db;

    private readonly PasswordHasher<User> _passwordHasher;

    private readonly IConfiguration _configuration;

    public AuthService(RestaurantDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        var userExists = await _db.Users
            .AnyAsync(u => u.Username == dto.Username);

        if (userExists)
        {
            return false;
        }

        var user = new User
        {
            Username = dto.Username,
            Role = "User"
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null)
        {
            return null;
        }

        if (!user.IsActive)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password
        );

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!
            ));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}