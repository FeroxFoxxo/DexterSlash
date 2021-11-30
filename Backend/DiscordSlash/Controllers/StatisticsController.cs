using DexterSlash.Databases.Repositories;
using DexterSlash.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DexterSlash.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {

        private readonly StatusRepository _status;
        private readonly OAuthManager _discordAuthManager;

        public StatisticsController (IServiceProvider serviceProvider)
        {
            _discordAuthManager = serviceProvider.GetRequiredService<OAuthManager>();
            _status = new (serviceProvider);
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            var identity = await _discordAuthManager.GetIdentity(HttpContext);

            return Ok(new
            {
                botStatus = _status.GetBotStatus(),
                dbStatus = await _status.GetDbStatus(),
                username = identity.RestClient.CurrentUser.Username,
                uptime = DateTime.Now - Process.GetCurrentProcess().StartTime
            });
        }

    }
}
