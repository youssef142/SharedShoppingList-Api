using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using FamilyShoppingList.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FamilyShoppingList.Endpoints
{
    public static class AnalyticsEndpoints
    {
        public static void MapAnalyticsEndpoints(this WebApplication app)
        {
            app.MapGet("groups/{groupId:guid}/stats", async (
                Guid groupId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = await db.GroupMembers.AnyAsync(gm =>
                    gm.UserId == userId && gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

                var items = db.ShoppingItems.Where(si => si.GroupId == groupId);

                var totalItems = await items.CountAsync();
                var totalBought = await items.CountAsync(i => i.Status == ItemStatus.Bought);
                var totalPending = await items.CountAsync(i => i.Status == ItemStatus.Pending);
                var totalCancelled = await items.CountAsync(i => i.Status == ItemStatus.Cancelled);

                // Per-user: how many they added vs how many they marked bought
                var addedByUser = await items
                    .Where(i => i.AddedByUserId != null)
                    .GroupBy(i => i.AddedByUser!.Username)
                    .Select(g => new { Username = g.Key, Count = g.Count() })
                    .ToListAsync();

                var boughtByUser = await items
                    .Where(i => i.Status == ItemStatus.Bought && i.StatusChangedByUserId != null)
                    .GroupBy(i => i.StatusChangedByUser!.Username)
                    .Select(g => new { Username = g.Key, Count = g.Count() })
                    .ToListAsync();

                var byUser = addedByUser
                    .Select(a => new UserStats
                    {
                        Username = a.Username,
                        AddedCount = a.Count,
                        BoughtCount = boughtByUser
                            .FirstOrDefault(b => b.Username == a.Username)?.Count ?? 0
                    })
                    .Union(boughtByUser
                        .Where(b => !addedByUser.Any(a => a.Username == b.Username))
                        .Select(b => new UserStats
                        {
                            Username = b.Username,
                            AddedCount = 0,
                            BoughtCount = b.Count
                        }))
                    .OrderByDescending(u => u.BoughtCount)
                    .ToList();

                // Most bought products (by name)
                var topProducts = await items
                    .Where(i => i.Status == ItemStatus.Bought)
                    .GroupBy(i => i.Name)
                    .Select(g => new ProductStats { Name = g.Key, BoughtCount = g.Count() })
                    .OrderByDescending(p => p.BoughtCount)
                    .Take(10)
                    .ToListAsync();

                // Most bought categories
                var topCategories = await items
                    .Where(i => i.Status == ItemStatus.Bought)
                    .GroupBy(i => i.Category != null ? i.Category.Name : "Uncategorized")
                    .Select(g => new CategoryStats { CategoryName = g.Key, BoughtCount = g.Count() })
                    .OrderByDescending(c => c.BoughtCount)
                    .ToListAsync();

                var response = new GroupStatsResponse
                {
                    TotalItems = totalItems,
                    TotalBought = totalBought,
                    TotalPending = totalPending,
                    TotalCancelled = totalCancelled,
                    ByUser = byUser,
                    TopProducts = topProducts,
                    TopCategories = topCategories
                };

                return Results.Ok(response);
            })
            .WithName("GetGroupStats")
            .RequireAuthorization();
        }
    }
}