using RestaurantApi.DTOs;

namespace RestaurantApi.Services.Auth;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterDto dto);

    Task<string?> LoginAsync(LoginDto dto);
}