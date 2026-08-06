using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FamilyShoppingList.Extensions;

namespace FamilyShoppingList.Endpoints
{
    public static class ItemEndpoints
    {
        public static void MapItemEndpoints(this WebApplication app)
        {
            app.MapGet("groups/{groupId:guid}/items", async (Guid groupId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();


                var isMember = await db.GroupMembers.AnyAsync(gm =>
                    gm.UserId == userId &&
                    gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

                var shoppingItems = await db.ShoppingItems
                    .Where(si => si.GroupId == groupId)
                    .Select(si => new ShoppingItemResponse
                    {
                        Id = si.Id,
                        Name = si.Name,
                        Quantity = si.Quantity,
                        AddedDate = si.AddedDate,
                        Status = si.Status.ToString(),
                        StatusDate = si.StatusDate,
                        AddedByUsername = si.AddedByUser != null
                            ? si.AddedByUser.Username
                            : null,
                        StatusChangedByUsername = si.StatusChangedByUser != null
                            ? si.StatusChangedByUser.Username
                            : null
                    })
                    .ToListAsync();

                return Results.Ok(shoppingItems);
            })
            .WithName("GetGroupItems")
            .RequireAuthorization();

            app.MapPost("groups/{groupId:guid}/items", async (Guid groupId, ShoppingItemCreateRequest request, ShoppingListDbContext db, ClaimsPrincipal user) =>
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

                var isMember = await db.GroupMembers.AnyAsync(gm =>
                    gm.UserId == userId &&
                    gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

                var item = new ShoppingItem
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Quantity = request.Quantity,
                    AddedDate = DateTime.Now,
                    Status = ItemStatus.Pending,
                    AddedByUserId = userId,
                    GroupId = groupId
                };

                db.ShoppingItems.Add(item);

                await db.SaveChangesAsync();

                var response = new ShoppingItemResponse
                {
                    Id = item.Id,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    AddedDate = item.AddedDate,
                    Status = item.Status.ToString(),
                    StatusDate = item.StatusDate,
                    AddedByUsername = user.GetUsername()
                };
                return Results.Created($"/groups/{groupId}/items/{item.Id}", response);
            })
            .WithName("CreateGroupItem")
            .RequireAuthorization();

            app.MapPatch("groups/{groupId:guid}/items/{itemId:Guid}/buy", async (Guid groupId, Guid itemId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = db.GroupMembers.Any(gm =>
                    gm.UserId == userId &&
                    gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

                var item = await db.ShoppingItems
                    .Include(i => i.AddedByUser)
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item is null)
                {
                    return Results.NotFound();
                }
                if (item.Status == ItemStatus.Pending)
                {
                    item.Status = ItemStatus.Bought;
                    item.StatusDate = DateTime.Now;
                    item.StatusChangedByUserId = userId;

                    ShoppingItemResponse responseItem = new ShoppingItemResponse
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Quantity = item.Quantity,
                        AddedDate = item.AddedDate,
                        StatusDate = item.StatusDate,
                        Status = item.Status.ToString(),
                        AddedByUsername = item.AddedByUser?.Username ?? "Unknown",
                        StatusChangedByUsername = user.GetUsername()
                    };

                    await db.SaveChangesAsync();

                    return Results.Ok(responseItem);
                }
                else
                {
                    return Results.Conflict($"Item is already {item.Status}, cannot mark as Bought.");
                }
            })
            .WithName("MarkItemBought")
            .RequireAuthorization();

            app.MapPatch("groups/{groupId:guid}/items/{itemId:Guid}/cancel", async (Guid groupId, Guid itemId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = db.GroupMembers.Any(gm =>
                    gm.UserId == userId &&
                    gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

                var item = await db.ShoppingItems
                    .Include(i => i.AddedByUser)
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item is null)
                {
                    return Results.NotFound();
                }
                if (item.Status == ItemStatus.Pending)
                {
                    item.Status = ItemStatus.Cancelled;
                    item.StatusDate = DateTime.Now;
                    item.StatusChangedByUserId = userId;

                    ShoppingItemResponse responseItem = new ShoppingItemResponse
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Quantity = item.Quantity,
                        AddedDate = item.AddedDate,
                        StatusDate = item.StatusDate,
                        Status = item.Status.ToString(),
                        AddedByUsername = item.AddedByUser?.Username ?? "Unknown",
                        StatusChangedByUsername = user.GetUsername()
                    };

                    await db.SaveChangesAsync();

                    return Results.Ok(responseItem);
                }
                else
                {
                    return Results.Conflict($"Item is already {item.Status}, cannot mark as Cancelled.");
                }
            })
            .WithName("MarkItemCancelled")
            .RequireAuthorization();

        }
    }
}
