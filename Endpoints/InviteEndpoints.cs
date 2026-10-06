using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using FamilyShoppingList.Models;
using System.Security.Claims;
using FamilyShoppingList.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FamilyShoppingList.Endpoints
{
    public static class InviteEndpoints
    {
        public static void MapInviteEndpoints(this WebApplication app)
        {
            app.MapPost("groups/{groupId:guid}/invites", async (Guid groupId, InviteRequest request, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var result = await db.Users
                    .Where(u => u.Username == request.Username)
                    .Select(u => new
                    {
                        InvitedUserId = u.Id,
                        RequesterIsMember = db.GroupMembers.Any(gm =>
                            gm.GroupId == groupId &&
                            gm.UserId == userId),

                        InviteAlreadyExists = db.GroupInvites.Any(i =>
                            i.GroupId == groupId &&
                            i.InvitedUserId == u.Id &&
                            i.InviteStatus == InviteStatus.Pending),

                        InvitedIsMember = db.GroupMembers.Any(gm =>
                            gm.GroupId == groupId &&
                            gm.UserId == u.Id),

                        GroupName = db.Groups
                            .Where(g => g.Id == groupId)
                            .Select(g => g.Name)
                            .FirstOrDefault()
                    })
                    .SingleOrDefaultAsync();

                if (result is null)
                    return Results.NotFound(new
                    {
                        message = "Invitation failed.",
                        errors = new[] { "User not found." }
                    });

                if (!result.RequesterIsMember)
                    return Results.Forbid();
                
                if (result.GroupName == null)
                    return Results.BadRequest(new
                    {
                        message = "Invitation failed.",
                        errors = new[] { "Group not found." }
                    });

                if (result.InviteAlreadyExists)
                    return Results.BadRequest(new
                    {
                        message = "Invitation failed.",
                        errors = new[] { "User already has a pending invite." }
                    });

                if (result.InvitedIsMember)
                    return Results.BadRequest(new
                    {
                        message = "Invitation failed.",
                        errors = new[] { "Invited user is already a member." }
                    });



                var invite = new GroupInvite
                {
                    GroupId = groupId,
                    InvitedUserId = result.InvitedUserId,
                    InvitingUserId = userId.Value,
                    InviteStatus = InviteStatus.Pending
                };

                db.GroupInvites.Add(invite);
                await db.SaveChangesAsync();

                var response = new InviteResponse
                {
                    Id = invite.Id,
                    GroupId = invite.GroupId,
                    GroupName = result.GroupName,
                    InvitedUsername = request.Username,
                    InvitingUsername = user.GetUsername(),
                    InviteStatus = invite.InviteStatus,
                    CreatedAt = invite.CreatedAt
                };
                return Results.Created(
                $"/groups/{groupId}/invites/{invite.Id}",
                response);
            })
            .WithName("CreateGroupInvite")
            .RequireAuthorization();

            app.MapGet("groups/{groupId:guid}/invites", async (Guid groupId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {

                var userId = user.GetUserId();

                if (userId == null)
                    return Results.Unauthorized();

                var isMember = await db.GroupMembers
                    .AnyAsync(gm =>
                        gm.GroupId == groupId &&
                        gm.UserId == userId);

                if (!isMember)
                    return Results.Forbid();

                var invites = await db.GroupInvites
                    .Where(i => i.GroupId == groupId)
                    .Select(i => new InviteResponse
                    {
                        Id = i.Id,
                        GroupId = i.GroupId,
                        GroupName = i.Group!.Name,
                        InvitedUsername = i.InvitedUser!.Username,
                        InvitingUsername = i.InvitingUser!.Username,
                        InviteStatus = i.InviteStatus,
                        CreatedAt = i.CreatedAt
                    })
                    .ToListAsync();

                return Results.Ok(invites);
            })
            .WithName("GetGroupInvites")
            .RequireAuthorization();

            app.MapGet("users/invites", async (ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();

                if (userId == null)
                    return Results.Unauthorized();

                var invites = await db.GroupInvites
                    .Where(i => i.InvitedUserId == userId &&
                                i.InviteStatus == InviteStatus.Pending)
                    .Select(i => new InviteResponse
                    {
                        Id = i.Id,
                        GroupId = i.GroupId,
                        GroupName = i.Group!.Name,
                        InvitedUsername = i.InvitedUser!.Username,
                        InvitingUsername = i.InvitingUser!.Username,
                        InviteStatus = i.InviteStatus,
                        CreatedAt = i.CreatedAt
                    })
                    .ToListAsync();

                return Results.Ok(invites);
            })
            .WithName("GetUserInvites")
            .RequireAuthorization();

            app.MapPatch("users/invites/{inviteId:guid}/{action}", async (Guid inviteId, string action, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                action = action.ToLowerInvariant();

                if (action != "accept" && action != "decline")
                    return Results.BadRequest(new
                    {
                        message = "Invitation failed.",
                        errors = new[] { "Invalid action. Use 'accept' or 'decline'." }
                    });

                // check user token and get user id
                var userId = user.GetUserId();

                if (userId == null)
                    return Results.Unauthorized();

                // get the invite and check if the user is not a member of the group already
                var result = await db.GroupInvites
                    .Where(i =>
                        i.Id == inviteId &&
                        i.InvitedUserId == userId &&
                        i.Group != null)
                    .Select(i => new
                    {
                        Invite = i,
                        AlreadyMember = i.Group!.Members.Any(m => m.UserId == userId)
                    })
                    .FirstOrDefaultAsync();

                // check if user owns this invite
                var invite = result?.Invite;

                if (invite == null)
                    return Results.Forbid();

                // check if the invite is still pending
                if (invite.InviteStatus != InviteStatus.Pending)
                    return Results.BadRequest(new
                    {
                        message = "Invitation response failed.",
                        errors = new[] { "Invalid invite status." }
                    });

                // check if the user is already a member of the group
                var alreadyMember = result?.AlreadyMember ?? false;

                if (alreadyMember)
                    return Results.BadRequest(new
                    {
                        message = "Joining group failed.",
                        errors = new[] { "You are already a member of this group." }
                    });


                if (action == "accept")
                {
                    invite.InviteStatus = InviteStatus.Accepted;
                    var groupMember = new GroupMember
                    {
                        Id = Guid.NewGuid(),
                        GroupId = invite.GroupId,
                        UserId = userId.Value
                    };
                    db.GroupMembers.Add(groupMember);
                }
                else
                {
                    invite.InviteStatus = InviteStatus.Declined;

                }

                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("UpdateInviteStatus")
            .RequireAuthorization();

        }
    }
}
