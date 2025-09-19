using Api.Application.Abstractions;
using Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrivateChatsController
        (IDbContext dbContext,
        ICurrentUserService currentUserService)
        : ControllerBase
    {

        // GET: api/<PrivateChatsController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = currentUserService.UserId ?? 0;
            var privateChats = await dbContext.Set<PrivateChat>()
                .Where(pc => pc.User1Id == userId || pc.User2Id == userId)
                .ToListAsync();

            return Ok(privateChats);

        }

        // GET api/<PrivateChatsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = currentUserService.UserId ?? 0;
            var privateChat = await dbContext.Set<PrivateChat>()
                .FindAsync(id);
            if (privateChat == null)
            {
                return NotFound();
            }
            if (privateChat.User1Id != userId && privateChat.User2Id != userId)
            {
                return Forbid();
            }
            return Ok(privateChat);
        }

        [HttpGet("with/{userId}")]
        public async Task<IActionResult> GetWithUser(int userId)
        {
            var currentUserId = currentUserService.UserId ?? 0;
            if (userId == currentUserId)
            {
                return BadRequest("Cannot create a private chat with yourself.");
            }

            var privateChat = await dbContext.Set<PrivateChat>()
                .Where(pc => (pc.User1Id == currentUserId && pc.User2Id == userId) ||
                             (pc.User1Id == userId && pc.User2Id == currentUserId))
                .FirstOrDefaultAsync();

            if (privateChat == null)
            {
                return NotFound();
            }
            return Ok(privateChat);
        }

        // POST api/<PrivateChatsController>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] string email)
        {
            var userId = currentUserService.UserId ?? 0;
            var userId2 = await dbContext.Set<User>()
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId2 == 0 || userId2 == userId)
            {
                return BadRequest("Invalid user Email");
            }
            var existingChat = await dbContext.Set<PrivateChat>()
                .Where(pc => (pc.User1Id == userId && pc.User2Id == userId2) ||
                             (pc.User1Id == userId2 && pc.User2Id == userId))
                .FirstOrDefaultAsync();
            if (existingChat != null)
            {
                var PrivateChat = new PrivateChat
                {
                    User1Id = userId,
                    User2Id = userId2,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow
                };
                await dbContext.Set<PrivateChat>().AddAsync(PrivateChat);
                await dbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = PrivateChat.Id }, PrivateChat);
            }
            return Ok(existingChat);

        }

        // DELETE api/<PrivateChatsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = currentUserService.UserId ?? 0;
            var privateChat = await dbContext.Set<PrivateChat>()
                .FindAsync(id);
            if (privateChat == null)
                return NotFound();

            if (privateChat.User1Id != userId && privateChat.User2Id != userId)
                return Forbid();
            dbContext.Set<PrivateChat>().Remove(privateChat);
            await dbContext.SaveChangesAsync();
            return NoContent();

        }
    }
}
