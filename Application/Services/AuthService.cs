using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IJwtTokenGenerator _jwtGen;

        public AuthService(
            UserManager<IdentityUser<Guid>> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IJwtTokenGenerator jwtGen)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtGen = jwtGen;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
        {
            // 1. Tạo user
            var user = new IdentityUser<Guid>
            {
                UserName = req.Email,
                Email = req.Email,
                EmailConfirmed = true
            };
            var createRes = await _userManager.CreateAsync(user, req.Password);
            if (!createRes.Succeeded)
                throw new Exception(string.Join("; ", createRes.Errors.Select(e => e.Description)));

            // 2. Gán role
            if (!await _roleManager.RoleExistsAsync(req.Role))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(req.Role));
            await _userManager.AddToRoleAsync(user, req.Role);

            // 3. Trả token
            var token = _jwtGen.GenerateToken(user);
            return new AuthResponse(token);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, req.Password))
                throw new Exception("Email hoặc mật khẩu không đúng");

            var token = _jwtGen.GenerateToken(user);
            return new AuthResponse(token);
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email)
                       ?? throw new Exception("User không tồn tại");
            var changeRes = await _userManager.ChangePasswordAsync(user, req.OldPassword, req.NewPassword);
            if (!changeRes.Succeeded)
                throw new Exception(string.Join("; ", changeRes.Errors.Select(e => e.Description)));
        }
    }
}