using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApi.Services;
using RestaurantApi.DTOs.Users;

namespace RestaurantApi.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly AdminUserService _adminUserService;

    public AdminUsersController(AdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _adminUserService.GetUsersAsync();

        return Ok(users);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(int id, UpdateUserRoleDto dto)
    {
        var user = await _adminUserService.UpdateUserRoleAsync(id, dto.Role);

        return Ok(user);
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var result = await _adminUserService
            .DeactivateUserAsync(id);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}