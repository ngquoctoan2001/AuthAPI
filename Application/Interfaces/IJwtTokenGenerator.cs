using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(IdentityUser<Guid> user);
    }
}
