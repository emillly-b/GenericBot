using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SaturnBot.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET: api/<UsersController>
        [HttpGet("/api/v1/guild/{guildId}/users")]
        [Authorize]
        public IActionResult Get(ulong guildId)
        {
            ulong userId = ulong.Parse(User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value);

            if (!Permissions.IsPermitted(userId, guildId, Permissions.PermissionLevels.Moderator))
                return new StatusCodeResult(403);

            return new JsonResult(Core.GetAllUsers(guildId));
        }
    }
}
