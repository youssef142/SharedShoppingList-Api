using FamilyShoppingList.DTO;
using FamilyShoppingList.Data;
using FamilyShoppingList.Extensions;
using FamilyShoppingList.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyShoppingList.Controllers;

[ApiController]
[Authorize]
[Route("groups")]
public class GroupsController(ShoppingListDbContext db) : ControllerBase
{
    [HttpPost(Name = "CreateGroup")]
    public async Task<IActionResult> CreateGroup(GroupUpdateRequest request)
    {
        var userId = User.GetUserId();
        var username = User.GetUsername();

        if (username == null || userId == null)
            return Unauthorized();

        var errors = request.ValidateModel();

        if (errors is not null)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors
            });
        }

        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            OwnerUserId = userId.Value,
        };

        db.Groups.Add(group);

        var groupMember = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId.Value,
        };

        // or 
        /*

        group.Members.Add(new GroupMember
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value
        });

        */

        db.GroupMembers.Add(groupMember);

        await db.SaveChangesAsync();

        var response = new GroupWithOwnerNameResponse
        {
            Id = group.Id,
            Name = group.Name,
            OwnerUsername = username,
            CreatedAt = group.CreatedAt
        };

        return Created($"/groups/{group.Id}", response);
    }

    [HttpGet(Name = "GetGroups")]
    public async Task<IActionResult> GetGroups()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var groups = await db.GroupMembers
            .Where(gm => gm.UserId == userId)
            .Select(gm => new GroupWithOwnerNameResponse
            {
                Id = gm.Group.Id,
                Name = gm.Group.Name,
                CreatedAt = gm.Group.CreatedAt,
                OwnerUsername = gm.Group.OwnerUser.Username
            })
            .ToListAsync();

        return Ok(groups);
    }

    [HttpPatch("{groupId:guid}", Name = "UpdateGroup")]
    public async Task<IActionResult> UpdateGroup(Guid groupId, GroupUpdateRequest request)
    {
        var errors = request.ValidateModel();

        if (errors is not null)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors
            });
        }

        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var group = await db.Groups
            .Include(g => g.OwnerUser)
            .FirstOrDefaultAsync(g =>
                g.Id == groupId &&
                g.OwnerUserId == userId);

        if (group == null)
            return Forbid();

        group.Name = request.Name;

        await db.SaveChangesAsync();

        return Ok(new GroupWithOwnerNameResponse
        {
            Id = group.Id,
            Name = group.Name,
            OwnerUsername = group.OwnerUser.Username,
            CreatedAt = group.CreatedAt
        });
    }

    [HttpPost("{groupId:guid}/leave", Name = "LeaveGroup")]
    public async Task<IActionResult> LeaveGroup(Guid groupId)
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var membership = await db.GroupMembers
            .Include(m => m.Group)
            .FirstOrDefaultAsync(m =>
                m.GroupId == groupId &&
                m.UserId == userId);

        if (membership == null)
            return Forbid();

        if (membership.Group.OwnerUserId == userId)
            return BadRequest(new
            {
                message = "Leaving group failed.",
                errors = new[] { "The group owner cannot leave the group." }
            });

        db.GroupMembers.Remove(membership);

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{groupId:Guid}/members", Name = "GetGroupMembers")]
    public async Task<IActionResult> GetGroupMembers(Guid groupId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var isMember = db.GroupMembers.Any(gm =>
        gm.UserId == userId &&
        gm.GroupId == groupId);

        if (!isMember)
            return Forbid();

        var group = await db.Groups
        .Where(g => g.Id == groupId)
        .Select(g => new GroupWithMembersResponse
        {
            Id = g.Id,
            Name = g.Name,
            CreatedAt = g.CreatedAt,
            OwnerUsername = g.OwnerUser != null ? g.OwnerUser.Username : null,
            Members = g.Members.Select(m => new UserRegisterResponse
            {
                Id = m.User!.Id,
                Username = m.User.Username,
                Email = m.User.Email
            }).ToList()
        })
        .FirstOrDefaultAsync();

        return Ok(group);
    }
}
