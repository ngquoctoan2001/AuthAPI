using Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class ExternalAuthController : ControllerBase
{
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IJwtTokenGenerator _jwtGen;

    public ExternalAuthController(
        SignInManager<IdentityUser<Guid>> signInManager,
        UserManager<IdentityUser<Guid>> userManager,
        IJwtTokenGenerator jwtGen)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtGen = jwtGen;
    }
    // https://localhost:7073/api/ExternalAuth/signin/Google?returnUrl=/api/ExternalAuth/profile

    // 1. Khởi động OAuth flow
    [HttpGet("signin/{provider}")]
    public IActionResult SignIn([FromRoute] string provider, [FromQuery] string returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(Callback), new { provider, returnUrl });
        var props = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(props, provider);
    }

    // 2. Callback nhận thông tin từ provider
    [HttpGet("callback/{provider}")]
    public async Task<IActionResult> Callback([FromRoute] string provider, [FromQuery] string returnUrl = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return BadRequest("Error loading external login info.");

        // 2a. Nếu user đã có login, sign in
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);
        IdentityUser<Guid> user;

        if (!signInResult.Succeeded)
        {
            // 2b. Nếu chưa có user, tạo mới
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null) return BadRequest("Email claim not received.");

            user = await _userManager.FindByEmailAsync(email)
                   ?? new IdentityUser<Guid> { UserName = email, Email = email };

            if (user.Id == Guid.Empty)
                await _userManager.CreateAsync(user);

            await _userManager.AddLoginAsync(user, info);
        }
        else
        {
            user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        }

        // 3. Phát JWT
        var token = _jwtGen.GenerateToken(user);
        // 4. Trả về token hoặc redirect về client
        return Ok(new { token, returnUrl });
    }
}
