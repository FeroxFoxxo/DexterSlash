using DiscordSlash.Identity;
using DiscordSlash.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordSlash.Controllers
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
            return Ok(new
            {
                botStatus = _status.GetBotStatus(),
                dbStatus = await _status.GetDbStatus()
            });
        }

    }
}
