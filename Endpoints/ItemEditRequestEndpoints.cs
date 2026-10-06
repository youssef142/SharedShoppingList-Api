using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using FamilyShoppingList.Extensions;
using FamilyShoppingList.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace FamilyShoppingList.Endpoints
{
    public static class ItemEditRequestEndpoints
    {
        public static void MapItemEditRequestEndpoints(this WebApplication app)
        {
            // Member creates a request
            app.MapPost("groups/{groupId:guid}/items/{itemId:guid}/edit-requests", async (
                Guid groupId, Guid itemId, CreateEditRequestRequest request,
                ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = await db.GroupMembers.AnyAsync(gm =>
                    gm.UserId == userId && gm.GroupId == groupId);
                if (!isMember)
                    return Results.Forbid();

                var item = await db.ShoppingItems
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.GroupId == groupId);
                if (item is null)
                    return Results.NotFound();

                   if (!Enum.TryParse<EditRequestType>(request.RequestType, true, out var requestType))
                        return Results.BadRequest(new
                        {
                            message = "Edit request creation failed.",
                            errors = new[] { "Invalid RequestType. Use 'Category' or 'Status'." }
                        });

                var editRequest = new ItemEditRequest
                {
                    Id = Guid.NewGuid(),
                    ItemId = itemId,
                    GroupId = groupId,
                    RequestedByUserId = userId.Value,
                    RequestType = requestType
                };

                if (requestType == EditRequestType.Category)
                {
                    if (request.ProposedCategoryId is not null)
                    {
                        var validCategory = await db.Categories.AnyAsync(c =>
                            c.Id == request.ProposedCategoryId && c.GroupId == groupId);
                        if (!validCategory)
                            return Results.BadRequest(new
                            {
                                message = "Category edit request failed.",
                                errors = new[] { "Invalid category for this group." }
                            });
                    }

                    if (request.ProposedCategoryId == item.CategoryId)
                        return Results.BadRequest(new
                        {
                            message = "Category edit request failed.",
                            errors = new[] { "Proposed category matches current category." }
                        });

                    editRequest.PreviousCategoryId = item.CategoryId;
                    editRequest.ProposedCategoryId = request.ProposedCategoryId;
                }
                else if (requestType == EditRequestType.Status)// Status
                {
                    if (string.IsNullOrWhiteSpace(request.ProposedStatus) ||
                        !Enum.TryParse<ItemStatus>(request.ProposedStatus, true, out var proposedStatus))
                        return Results.BadRequest(new
                        {
                            message = "Status edit request failed.",
                            errors = new[] { "Invalid ProposedStatus." }
                        });

                    if (proposedStatus == item.Status)
                        return Results.BadRequest(new
                        {
                            message = "Status edit request failed.",
                            errors = new[] { "Proposed status matches current status." }
                        });

                    editRequest.PreviousStatus = item.Status;
                    editRequest.ProposedStatus = proposedStatus;
                }
                else if (requestType == EditRequestType.AddedBy)
                {
                    if (request.ProposedUserId is null)
                        return Results.BadRequest(new
                        {
                            message = "AddedBy edit request failed.",
                            errors = new[] { "ProposedUserId is required." }
                        });

                    var isProposedMember = await db.GroupMembers.AnyAsync(gm =>
                        gm.UserId == request.ProposedUserId && gm.GroupId == groupId);
                    if (!isProposedMember)
                        return Results.BadRequest(new
                        {
                            message = "AddedBy edit request failed.",
                            errors = new[] { "Proposed user is not a member of this group." }
                        });

                    if (request.ProposedUserId == item.AddedByUserId)
                        return Results.BadRequest(new
                        {
                            message = "AddedBy edit request failed.",
                            errors = new[] { "Proposed user matches current value." }
                        });

                    editRequest.PreviousAddedByUserId = item.AddedByUserId;
                    editRequest.ProposedAddedByUserId = request.ProposedUserId;
                }
                else if (requestType == EditRequestType.StatusChangedBy)
                {
                    if (item.Status == ItemStatus.Pending)
                        return Results.BadRequest(new
                        {
                            message = "StatusChangedBy edit request failed.",
                            errors = new[] { "Item has no buyer/canceller to reassign yet." }
                        });

                    if (request.ProposedUserId is null)
                        return Results.BadRequest(new
                        {
                            message = "StatusChangedBy edit request failed.",
                            errors = new[] { "ProposedUserId is required." }
                        });

                    var isProposedMember = await db.GroupMembers.AnyAsync(gm =>
                        gm.UserId == request.ProposedUserId && gm.GroupId == groupId);
                    if (!isProposedMember)
                        return Results.BadRequest(new
                        {
                            message = "StatusChangedBy edit request failed.",
                            errors = new[] { "Proposed user is not a member of this group." }
                        });

                    if (request.ProposedUserId == item.StatusChangedByUserId)
                        return Results.BadRequest(new
                        {
                            message = "StatusChangedBy edit request failed.",
                            errors = new[] { "Proposed user matches current value." }
                        });

                    editRequest.PreviousStatusChangedByUserId = item.StatusChangedByUserId;
                    editRequest.ProposedStatusChangedByUserId = request.ProposedUserId;
                }

                db.ItemEditRequests.Add(editRequest);
                await db.SaveChangesAsync();

                return Results.Created($"/groups/{groupId}/edit-requests/{editRequest.Id}",
                    new { editRequest.Id });
            })
            .WithName("CreateItemEditRequest")
            .RequireAuthorization();

            // All requests for a group (any member can view)
            app.MapGet("groups/{groupId:guid}/edit-requests", async (
                Guid groupId, ShoppingListDbContext db, ClaimsPrincipal user, string? status) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = await db.GroupMembers.AnyAsync(gm =>
                    gm.UserId == userId && gm.GroupId == groupId);
                if (!isMember)
                    return Results.Forbid();

                var query = db.ItemEditRequests.Where(r => r.GroupId == groupId);

                if (!string.IsNullOrWhiteSpace(status) &&
                    Enum.TryParse<EditRequestStatus>(status, true, out var statusFilter))
                {
                    query = query.Where(r => r.RequestStatus == statusFilter);
                }

                var requests = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(ToResponse)
                    .ToListAsync();

                return Results.Ok(requests);
            })
            .WithName("GetGroupEditRequests")
            .RequireAuthorization();

            // Requests for a specific item
            app.MapGet("groups/{groupId:guid}/items/{itemId:guid}/edit-requests", async (
                Guid groupId, Guid itemId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = await db.GroupMembers.AnyAsync(gm =>
                    gm.UserId == userId && gm.GroupId == groupId);
                if (!isMember)
                    return Results.Forbid();

                var requests = await db.ItemEditRequests
                    .Where(r => r.ItemId == itemId && r.GroupId == groupId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(ToResponse)
                    .ToListAsync();

                return Results.Ok(requests);
            })
            .WithName("GetItemEditRequests")
            .RequireAuthorization();

            // Owner approves
            app.MapPatch("groups/{groupId:guid}/edit-requests/{requestId:guid}/approve", async (
                Guid groupId, Guid requestId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                if (group is null)
                    return Results.NotFound();

                if (group.OwnerUserId != userId)
                    return Results.Forbid();

                var editRequest = await db.ItemEditRequests
                    .Include(r => r.Item)
                    .FirstOrDefaultAsync(r => r.Id == requestId && r.GroupId == groupId);

                if (editRequest is null)
                    return Results.NotFound();

                if (editRequest.RequestStatus != EditRequestStatus.Pending)
                    return Results.Conflict(new
                    {
                        message = "Request processing failed.",
                        errors = new[] { $"Request is already {editRequest.RequestStatus}." }
                    });

                var item = editRequest.Item!;

                // staleness check: item must still be in the state the request was made against
                if (editRequest.RequestType == EditRequestType.Category &&
                    item.CategoryId != editRequest.PreviousCategoryId)
                {
                    return Results.Conflict(new
                    {
                        message = "Request approval failed.",
                        errors = new[] { "Item's category has changed since this request was made. Decline it and ask for a new request." }
                    });

                }

                if (editRequest.RequestType == EditRequestType.Status &&
                    item.Status != editRequest.PreviousStatus)
                {
                    return Results.Conflict(new
                    {
                        message = "Request approval failed.",
                        errors = new[] { "Item's status has changed since this request was made. Decline it and ask for a new request." }
                    });

                }

                if (editRequest.RequestType == EditRequestType.AddedBy &&
                    item.AddedByUserId != editRequest.PreviousAddedByUserId)
                {
                    return Results.Conflict(new
                    {
                        message = "Request approval failed.",
                        errors = new[] { "Item's AddedBy has changed since this request was made." }
                    });
                }


                if (editRequest.RequestType == EditRequestType.StatusChangedBy &&
                    item.StatusChangedByUserId != editRequest.PreviousStatusChangedByUserId)
                {
                    return Results.Conflict(new
                    {
                        message = "Request approval failed.",
                        errors = new[] { "Item's buyer/canceller has changed since this request was made." }
                    });
                }




                if (editRequest.RequestType == EditRequestType.Category)
                {
                    item.CategoryId = editRequest.ProposedCategoryId;
                }
                else if (editRequest.RequestType == EditRequestType.Status)
                {
                    item.Status = editRequest.ProposedStatus!.Value;

                    if (editRequest.ProposedStatus == ItemStatus.Pending)
                    {
                        // reopening: no one "did" anything, so clear both
                        item.StatusDate = null;
                        item.StatusChangedByUserId = null;
                    }
                    else
                    {
                        // proposed status is Bought or Cancelled: this is a real action,
                        // so stamp when it happened and who's responsible
                        item.StatusDate = DateTime.UtcNow;
                        item.StatusChangedByUserId = editRequest.RequestedByUserId; // credit goes to requester
                    }
                }

                else if (editRequest.RequestType == EditRequestType.AddedBy)
                {
                    item.AddedByUserId = editRequest.ProposedAddedByUserId;
                }
                else if (editRequest.RequestType == EditRequestType.StatusChangedBy)
                {
                    item.StatusChangedByUserId = editRequest.ProposedStatusChangedByUserId;
                }

                editRequest.RequestStatus = EditRequestStatus.Approved;
                editRequest.ReviewedAt = DateTime.UtcNow;
                editRequest.ReviewedByUserId = userId;

                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    message = "Request approved.",
                    errors = Array.Empty<string>()
                });

            })
            .WithName("ApproveEditRequest")
            .RequireAuthorization();

            // Owner declines
            app.MapPatch("groups/{groupId:guid}/edit-requests/{requestId:guid}/decline", async (
                Guid groupId, Guid requestId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                if (group is null)
                    return Results.NotFound();

                if (group.OwnerUserId != userId)
                    return Results.Forbid();

                var editRequest = await db.ItemEditRequests
                    .FirstOrDefaultAsync(r => r.Id == requestId && r.GroupId == groupId);

                if (editRequest is null)
                    return Results.NotFound();

                if (editRequest.RequestStatus != EditRequestStatus.Pending)
                    return Results.Conflict(new
                    {
                        message = "Request processing failed.",
                        errors = new[] { $"Request is already {editRequest.RequestStatus}." }
                    });

                editRequest.RequestStatus = EditRequestStatus.Declined;
                editRequest.ReviewedAt = DateTime.UtcNow;
                editRequest.ReviewedByUserId = userId;

                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    message = "Request declined.",
                    errors = Array.Empty<string>()
                });

            })
            .WithName("DeclineEditRequest")
            .RequireAuthorization();
        }

        private static readonly Expression<Func<ItemEditRequest, ItemEditRequestResponse>> ToResponse = r => new ItemEditRequestResponse
        {
            Id = r.Id,
            ItemId = r.ItemId,
            ItemName = r.Item != null ? r.Item.Name : string.Empty,
            RequestedByUsername = r.RequestedByUser != null ? r.RequestedByUser.Username : "Unknown",
            RequestType = r.RequestType.ToString(),

            PreviousCategoryId = r.PreviousCategoryId,
            PreviousCategoryName = r.PreviousCategoryId != null
                ? r.Item!.Group!.Categories.Where(c => c.Id == r.PreviousCategoryId).Select(c => c.Name).FirstOrDefault()
                : null,
            ProposedCategoryId = r.ProposedCategoryId,
            ProposedCategoryName = r.ProposedCategoryId != null
                ? r.Item!.Group!.Categories.Where(c => c.Id == r.ProposedCategoryId).Select(c => c.Name).FirstOrDefault()
                : null,

            PreviousStatus = r.PreviousStatus != null ? r.PreviousStatus.ToString() : null,
            ProposedStatus = r.ProposedStatus != null ? r.ProposedStatus.ToString() : null,

            PreviousAddedByUserId = r.PreviousAddedByUserId,
            PreviousAddedByUsername = r.PreviousAddedByUserId != null
                ? r.Group!.Members.Where(m => m.UserId == r.PreviousAddedByUserId).Select(m => m.User!.Username).FirstOrDefault()
                : null,
            ProposedAddedByUserId = r.ProposedAddedByUserId,
            ProposedAddedByUsername = r.ProposedAddedByUserId != null
                ? r.Group!.Members.Where(m => m.UserId == r.ProposedAddedByUserId).Select(m => m.User!.Username).FirstOrDefault()
                : null,

            PreviousStatusChangedByUserId = r.PreviousStatusChangedByUserId,
            PreviousStatusChangedByUsername = r.PreviousStatusChangedByUserId != null
                ? r.Group!.Members.Where(m => m.UserId == r.PreviousStatusChangedByUserId).Select(m => m.User!.Username).FirstOrDefault()
                : null,
            ProposedStatusChangedByUserId = r.ProposedStatusChangedByUserId,
            ProposedStatusChangedByUsername = r.ProposedStatusChangedByUserId != null
                ? r.Group!.Members.Where(m => m.UserId == r.ProposedStatusChangedByUserId).Select(m => m.User!.Username).FirstOrDefault()
                : null,

            RequestStatus = r.RequestStatus.ToString(),
            CreatedAt = r.CreatedAt,
            ReviewedAt = r.ReviewedAt,
            ReviewedByUsername = r.ReviewedByUser != null ? r.ReviewedByUser.Username : null
        };
    }
}