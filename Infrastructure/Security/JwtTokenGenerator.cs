using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Application.Interfaces;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _config;
    private readonly UserManager<IdentityUser<Guid>> _userMgr;

    public JwtTokenGenerator(IConfiguration config, UserManager<IdentityUser<Guid>> userMgr)
    {
        _config = config; _userMgr = userMgr;
    }

    public string GenerateToken(IdentityUser<Guid> user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };
        // thêm role claims
        var roles = _userMgr.GetRolesAsync(user).GetAwaiter().GetResult();
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
