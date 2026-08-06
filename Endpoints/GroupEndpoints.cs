using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using FamilyShoppingList.Extensions;
using FamilyShoppingList.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FamilyShoppingList.Endpoints
{
    public static class GroupEndpoints
    {
        public static void MapGroupEndpoints(this WebApplication app)
        {
            app.MapPost("/groups", async (GroupUpdateRequest request, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {

                var userId = user.GetUserId();
                var username = user.GetUsername();

                if (username == null || userId == null)
                    return Results.Unauthorized();

                var errors = request.ValidateModel();

                if (errors is not null)
                {
                    return Results.BadRequest(new
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

                return Results.Created($"/groups/{group.Id}", response);
            })
            .WithName("CreateGroup")
            .RequireAuthorization();

            app.MapGet("/groups", async (ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

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

                return Results.Ok(groups);
            })
            .WithName("GetGroups")
            .RequireAuthorization();

            app.MapPatch("groups/{groupId:guid}", async (Guid groupId, GroupUpdateRequest request, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var errors = request.ValidateModel();

                if (errors is not null)
                {
                    return Results.BadRequest(new
                    {
                        message = "Validation failed.",
                        errors
                    });
                }

                var userId = user.GetUserId();

                if (userId == null)
                    return Results.Unauthorized();

                var group = await db.Groups
                    .Include(g => g.OwnerUser)
                    .FirstOrDefaultAsync(g =>
                        g.Id == groupId &&
                        g.OwnerUserId == userId);

                if (group == null)
                    return Results.Forbid();

                group.Name = request.Name;

                await db.SaveChangesAsync();

                return Results.Ok(new GroupWithOwnerNameResponse
                {
                    Id = group.Id,
                    Name = group.Name,
                    OwnerUsername = group.OwnerUser.Username,
                    CreatedAt = group.CreatedAt
                });
            })
            .WithName("UpdateGroup")
            .RequireAuthorization();

            app.MapPost("groups/{groupId:guid}/leave", async (Guid groupId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();

                if (userId == null)
                    return Results.Unauthorized();

                var membership = await db.GroupMembers
                    .Include(m => m.Group)
                    .FirstOrDefaultAsync(m =>
                        m.GroupId == groupId &&
                        m.UserId == userId);

                if (membership == null)
                    return Results.Forbid();

                if (membership.Group.OwnerUserId == userId)
                    return Results.BadRequest("The group owner cannot leave the group.");

                db.GroupMembers.Remove(membership);

                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("LeaveGroup")
            .RequireAuthorization();           

            app.MapGet("/groups/{groupId:Guid}/members", async (Guid groupId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = db.GroupMembers.Any(gm =>
                gm.UserId == userId &&
                gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

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



                return Results.Ok(group);
            })
            .WithName("GetGroupMembers")
            .RequireAuthorization();

        }
    }
}
