using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using FamilyShoppingList.Extensions;
using FamilyShoppingList.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;



namespace FamilyShoppingList.Endpoints
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this WebApplication app)
        {
            app.MapGet("/groups/{groupId:guid}/categories", async (
                Guid groupId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = await db.GroupMembers
                    .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

                var categories = await db.Categories
                    .Where(c => c.GroupId == groupId)
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryResponse
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToListAsync();

                return Results.Ok(categories);
            })
            .WithName("GetCategories")
            .RequireAuthorization();

            app.MapPost("/groups/{groupId:guid}/categories", async (
                Guid groupId, CategoryCreateRequest request,
                ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
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

                var isMember = await db.GroupMembers
                    .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

                var name = request.Name.Trim();

                var exists = await db.Categories
                    .AnyAsync(c => c.GroupId == groupId &&
                                   c.Name.ToLower() == name.ToLower());

                if (exists)
                    return Results.Conflict(new
                    {
                        message = "Category creation failed.",
                        errors = new[] { "Category already exists in this group." }
                    });

                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    GroupId = groupId
                };

                db.Categories.Add(category);
                await db.SaveChangesAsync();

                var response = new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name
                };

                return Results.Created($"/groups/{groupId}/categories/{category.Id}", response);
            })
            .WithName("CreateCategory")
            .RequireAuthorization();

            app.MapDelete("/groups/{groupId:guid}/categories/{categoryId:guid}", async (
                Guid groupId, Guid categoryId, ShoppingListDbContext db, ClaimsPrincipal user) =>
            {
                var userId = user.GetUserId();
                if (userId == null)
                    return Results.Unauthorized();

                var isMember = await db.GroupMembers
                    .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

                if (!isMember)
                    return Results.Forbid();

                var category = await db.Categories
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.GroupId == groupId);

                if (category is null)
                    return Results.NotFound();

                db.Categories.Remove(category);
                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteCategory")
            .RequireAuthorization();
        }
    }
}