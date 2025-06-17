using Application.DTOs;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest req);
        Task<AuthResponse> LoginAsync(LoginRequest req);
        Task ChangePasswordAsync(ChangePasswordRequest req);
    }

}
