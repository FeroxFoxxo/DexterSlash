using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordSlash.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        [HttpGet("login")]
        public IActionResult Login([FromQuery] string ReturnUrl)
        {
            if (string.IsNullOrEmpty(ReturnUrl))
            {
                ReturnUrl = "/api/status";
            }

            var properties = new AuthenticationProperties()
            {
                RedirectUri = ReturnUrl,
                Items =
                {
                    { "LoginProvider", "Discord" },
                    { "scheme", "Discord" }
                },
                AllowRefresh = true,
            };
            return Challenge(properties, "Discord");
        }

        [HttpGet("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var properties = new AuthenticationProperties()
            {
                Items =
                {
                    { "LoginProvider", "Discord" },
                    { "scheme", "Discord" }
                },
                AllowRefresh = true,
            };
            return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme);
        }

    }
}
