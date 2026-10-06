using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using FamilyShoppingList.Extensions;
using FamilyShoppingList.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                            : null,
                        CategoryId = si.CategoryId,
                        CategoryName = si.Category != null ? si.Category.Name : null
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

                Category? category = null;
                if (request.CategoryId is not null)
                {
                    category = await db.Categories
                        .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.GroupId == groupId);

                    if (category is null)
                        return Results.BadRequest(new
                        {
                            message = "Item creation failed.",
                            errors = new[] { "Invalid category for this group." }
                        });
                }
                var item = new ShoppingItem
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Quantity = request.Quantity,
                    AddedDate = DateTime.UtcNow,
                    Status = ItemStatus.Pending,
                    AddedByUserId = userId,
                    GroupId = groupId,
                    CategoryId = request.CategoryId
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
                    AddedByUsername = user.GetUsername(),
                    CategoryId = category?.Id,
                    CategoryName = category?.Name
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
                    .Include(i => i.Category)
                    .FirstOrDefaultAsync(i => i.Id == itemId && i.GroupId == groupId);

                if (item is null)
                {
                    return Results.NotFound();
                }
                if (item.Status == ItemStatus.Pending)
                {
                    item.Status = ItemStatus.Bought;
                    item.StatusDate = DateTime.UtcNow;
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
                        StatusChangedByUsername = user.GetUsername(),
                        CategoryId = item.CategoryId,
                        CategoryName = item.Category?.Name
                    };

                    await db.SaveChangesAsync();

                    return Results.Ok(responseItem);
                }
                else
                {
                    return Results.Conflict(new
                    {
                        message = "Item status update failed.",
                        errors = new[] { $"Item is already {item.Status}, cannot mark as Bought." }
                    });
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
                    .Include(i => i.Category)
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item is null)
                {
                    return Results.NotFound();
                }
                if (item.Status == ItemStatus.Pending)
                {
                    item.Status = ItemStatus.Cancelled;
                    item.StatusDate = DateTime.UtcNow;
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
                        StatusChangedByUsername = user.GetUsername(),
                        CategoryId = item.CategoryId,
                        CategoryName = item.Category?.Name
                    };

                    await db.SaveChangesAsync();

                    return Results.Ok(responseItem);
                }
                else
                {
                    return Results.Conflict(new
                    {
                        message = "Item status update failed.",
                        errors = new[] { $"Item is already {item.Status}, cannot mark as Cancelled." }
                    });
                }
            })
            .WithName("MarkItemCancelled")
            .RequireAuthorization();

            app.MapGet("groups/{groupId:guid}/items/suggestions", async (
    Guid groupId, string query, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = await db.GroupMembers.AnyAsync(gm =>
                    gm.UserId == userId && gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

                if (string.IsNullOrWhiteSpace(query))
                    return Results.Ok(new List<ItemSuggestionResponse>());

                var suggestions = await db.ShoppingItems
                    .Where(i => i.GroupId == groupId &&
                                i.Name.ToLower().StartsWith(query.ToLower()))
                    .GroupBy(i => i.Name)
                    .Select(g => new ItemSuggestionResponse
                    {
                        Name = g.Key,
                        TimesAdded = g.Count(),
                        // most recent category used for this item name, if any
                        CategoryId = g.OrderByDescending(i => i.AddedDate)
                                      .Select(i => i.CategoryId)
                                      .FirstOrDefault(),
                        CategoryName = g.OrderByDescending(i => i.AddedDate)
                                      .Select(i => i.Category != null ? i.Category.Name : null)
                                      .FirstOrDefault()
                    })
                    .OrderByDescending(s => s.TimesAdded)
                    .Take(10)
                    .ToListAsync();

                return Results.Ok(suggestions);
            })
.WithName("GetItemSuggestions")
.RequireAuthorization();

        }
    }
}
