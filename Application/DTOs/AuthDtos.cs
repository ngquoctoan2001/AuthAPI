namespace Application.DTOs
{
    public record RegisterRequest(string Email, string Password, string Role);
    public record LoginRequest(string Email, string Password);
    public record ChangePasswordRequest(string Email, string OldPassword, string NewPassword);
    public record AuthResponse(string Token);
}
