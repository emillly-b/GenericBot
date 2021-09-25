using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaturnBot.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // GET: api/v1/user/UserId
        [Authorize]
        [HttpGet("/api/v1/user")]
        public object Get()
        {
            ulong userId = ulong.Parse(User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
            var user = Core.DiscordClient.GetUser(userId);

            var userData = new {
                id = user.Id,
                username = user.Username,
                discriminator = user.Discriminator,
                guilds = Core.DiscordClient.Guilds.Where(g => g.Users.Any(u => u.Id == userId)).Select(g => new 
                { 
                    id = g.Id,
                    name = g.Name 
                })
            };

            return userData;
        }
    }
}
