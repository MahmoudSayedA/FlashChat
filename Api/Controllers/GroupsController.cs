using Api.Application.Abstractions;
using Api.Application.Dtos;
using Api.Constants;
using Api.Entities;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupsController(
        IDbContext dbContext,
        ICurrentUserService currentUserService
        ) : ControllerBase
    {
        [HttpGet]

        public async Task<IActionResult> Get()
        {
            var userId = currentUserService.UserId ?? 0;
            // get all group chats where the user is a member
            var userGroupIds = await dbContext.Set<GroupChatMember>()
                .Where(gcm => gcm.UserId == userId)
                .Select(gcm => gcm.GroupChatId)
                .ToListAsync();

            var groupChats = await dbContext.Set<GroupChat>()
                .Where(gc => userGroupIds.Contains(gc.Id))
                .ToListAsync();

            return Ok(groupChats);

        }

        // GET api/<GroupChatsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = currentUserService.UserId ?? 0;
            var groupChat = await dbContext.Set<GroupChat>()
                .FindAsync(id);
            if (groupChat == null)
            {
                return NotFound();
            }
            // get group members
            var membersIds = await dbContext.Set<GroupChatMember>()
                .Where(gcm => gcm.GroupChatId == id)
                .Select(gcm => gcm.UserId)
                .ToListAsync();

            // check if the user is a member
            if (!membersIds.Contains(userId))
            {
                return Forbid();
            }

            return Ok(groupChat);
        }

        // POST api/<GroupChatsController>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddGroupDto groupDto)
        {
            var userId = currentUserService.UserId ?? 0;

            var group = new GroupChat
            {
                Name = groupDto.GroupName,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };
            using var transaction = await dbContext.BeginTransactionAsync();
            try
            {

                // create group
                await dbContext.Set<GroupChat>().AddAsync(group);
                await dbContext.SaveChangesAsync();

                // get membersIds distinct
                var memberIds = await dbContext.Set<User>()
                    .Where(u => groupDto.MembersEmails.Contains(u.Email!))
                    .Select(u => u.Id)
                    .ToListAsync();

                // create group members
                var groupMembers = memberIds.Distinct().Select(id => new GroupChatMember
                {
                    GroupChatId = group.Id,
                    UserId = id,
                    Role = Roles.Member
                }).ToList();
                groupMembers.Add(new GroupChatMember
                {
                    GroupChatId = group.Id,
                    UserId = userId,
                    Role = Roles.Admin
                });
                await dbContext.Set<GroupChatMember>().AddRangeAsync(groupMembers);
                await dbContext.SaveChangesAsync();

                // commit transaction
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(Get), new { id = group.Id }, group);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPost("Members/{groupId}")]
        public async Task <IActionResult> AddMembers(int groupId, [FromBody] AddGroupDto addGroupDto)
        {
            // get group
            var userId = currentUserService.UserId ?? 0;
            var group = await dbContext.Set<GroupChat>()
                .FindAsync(groupId);
            if (group == null)
                return NotFound();

            // check if the user is an admin
            var isAdmin = await dbContext.Set<GroupChatMember>()
                .AnyAsync(gcm => gcm.GroupChatId == groupId && gcm.UserId == userId && gcm.Role == Roles.Admin);
            if (!isAdmin)
                return Forbid();

            // get membersIds distinct
            var memberIds = await dbContext.Set<User>()
                .Where(u => addGroupDto.MembersEmails.Contains(u.Email!))
                .Select(u => u.Id)
                .ToListAsync();

            // create group members
            var groupMembers = memberIds.Distinct().Select(id => new GroupChatMember
            {
                GroupChatId = group.Id,
                UserId = id,
                Role = Roles.Member
            }).ToList();

            await dbContext.Set<GroupChatMember>().AddRangeAsync(groupMembers);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Members/{groupId}")]
        public async Task<IActionResult> RemoveMember(int groupId, [FromBody] string memberEmail)
        {
            // get group
            var userId = currentUserService.UserId ?? 0;
            var group = await dbContext.Set<GroupChat>()
                .FindAsync(groupId);
            if (group == null)
                return NotFound();

            // check if the user is an admin
            var isAdmin = await dbContext.Set<GroupChatMember>()
                .AnyAsync(gcm => gcm.GroupChatId == groupId && gcm.UserId == userId && gcm.Role == Roles.Admin);
            if (!isAdmin)
                return Forbid();

            // get memberId
            var member = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == memberEmail);
            if (member == null)
                return NotFound();

            // remove group member
            var groupMember = await dbContext.Set<GroupChatMember>()
                .FirstOrDefaultAsync(gcm => gcm.GroupChatId == groupId && gcm.UserId == member.Id);
            if (groupMember == null)
                return NotFound();
            dbContext.Set<GroupChatMember>().Remove(groupMember);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("Members/{groupId}/MakeAdmin")]
        public async Task<IActionResult> MakeAdmin(int groupId, [FromBody] string memberEmail)
        {
            // get group
            var userId = currentUserService.UserId ?? 0;
            var group = await dbContext.Set<GroupChat>()
                .FindAsync(groupId);
            if (group == null)
                return NotFound();

            // check if the user is an admin
            var isAdmin = await dbContext.Set<GroupChatMember>()
                .AnyAsync(gcm => gcm.GroupChatId == groupId && gcm.UserId == userId && gcm.Role == Roles.Admin);
            if (!isAdmin)
                return Forbid();

            // get memberId
            var member = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == memberEmail);
            if (member == null)
                return NotFound();

            // make group member an admin
            var groupMember = await dbContext.Set<GroupChatMember>()
                .FirstOrDefaultAsync(gcm => gcm.GroupChatId == groupId && gcm.UserId == member.Id);
            if (groupMember == null)
                return NotFound();

            groupMember.Role = Roles.Admin;
            dbContext.Set<GroupChatMember>().Update(groupMember);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("Members/{groupId}/RevokeAdmin")]
        public async Task<IActionResult> RevokeAdmin(int groupId, [FromBody] string memberEmail)
        {
            // get group
            var userId = currentUserService.UserId ?? 0;
            var group = await dbContext.Set<GroupChat>()
                .FindAsync(groupId);
            if (group == null)
                return NotFound();

            // check if the user is an admin
            var isAdmin = await dbContext.Set<GroupChatMember>()
                .AnyAsync(gcm => gcm.GroupChatId == groupId && gcm.UserId == userId && gcm.Role == Roles.Admin);
            if (!isAdmin)
                return Forbid();

            // get memberId
            var member = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == memberEmail);
            if (member == null)
                return NotFound();

            // revoke admin role from group member
            var groupMembers = await dbContext.Set<GroupChatMember>()
                .Where(gcm => gcm.GroupChatId == groupId)
                .ToListAsync();
            var adminCount = groupMembers.Count(gcm => gcm.Role == Roles.Admin);
            if (adminCount <= 1)
                return BadRequest("Cannot revoke admin role. The group must have at least one admin.");
            
            var groupMember = groupMembers
                .FirstOrDefault(gcm => gcm.GroupChatId == groupId && gcm.UserId == member.Id);


            if (groupMember == null)
                return NotFound();

            groupMember.Role = Roles.Member;
            dbContext.Set<GroupChatMember>().Update(groupMember);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGroupDto updateGroupDto)
        {
            var userId = currentUserService.UserId ?? 0;
            var groupChat = await dbContext.Set<GroupChat>()
                .FindAsync(id);
            if (groupChat == null)
                return NotFound();

            // check if the user is an admin
            var isAdmin = await dbContext.Set<GroupChatMember>()
                .AnyAsync(gcm => gcm.GroupChatId == id && gcm.UserId == userId && gcm.Role == Roles.Admin);
            if (!isAdmin)
                return Forbid();

            groupChat.Name = updateGroupDto.GroupName ?? groupChat.Name;
            dbContext.Set<GroupChat>().Update(groupChat);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/<GroupChatsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = currentUserService.UserId ?? 0;
            var groupChat = await dbContext.Set<GroupChat>()
                .FindAsync(id);
            if (groupChat == null)
                return NotFound();

            // check if the user is the creator
            if (groupChat.CreatedById != userId)
                return Forbid();


            dbContext.Set<GroupChat>().Remove(groupChat);
            await dbContext.SaveChangesAsync();
            return NoContent();

        }
    }
}
