using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs.Users;
using RestaurantApi.Exceptions;

namespace RestaurantApi.Services;

public class AdminUserService
{
    private readonly RestaurantDbContext _db;

    public AdminUserService(RestaurantDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserResponseDto>> GetUsersAsync()
    {
        return await _db.Users
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role
            })
            .ToListAsync();
    }

    public async Task<UserResponseDto> UpdateUserRoleAsync(int userId, string role)
    {
        var normalizedRole = role.Trim();

        var allowedRoles = new[] { "User", "Admin" };

        if (!allowedRoles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Invalid role");
        }

        var user = await _db.Users.FindAsync(userId);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.Role = normalizedRole;

        await _db.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task<bool> DeactivateUserAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
        {
            return false;
        }

        user.IsActive = false;

        await _db.SaveChangesAsync();

        return true;
    }
}