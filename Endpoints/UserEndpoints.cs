using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using System.Security.Claims;
using FamilyShoppingList.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FamilyShoppingList.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            app.MapGet("users/me", async (ShoppingListDbContext db, ClaimsPrincipal user) =>
            {

                var userId = user.GetUserId();

                if (userId == null)
                    return Results.Unauthorized();

                var userDb = await db.Users.FindAsync(userId);

                if (userDb == null)
                    return Results.NotFound();

                var response = new UserProfileResponse
                {
                    Id = userDb.Id,
                    Username = userDb.Username,
                    DisplayName = userDb.DisplayName,
                    Email = userDb.Email,
                    JoinedAt = userDb.JoinedAt
                };

                return Results.Ok(response);
            })
            .WithName("GetCurrentUser")
            .RequireAuthorization();

            app.MapPatch("users/me", async (UserUpdateRequest request, ShoppingListDbContext db, ClaimsPrincipal user) =>
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

                var userDb = await db.Users.FindAsync(userId);

                if (userDb == null)
                    return Results.NotFound();

                var exists = await db.Users
                    .AnyAsync(x => x.Email == request.Email && x.Id != userId);

                if (exists)
                {
                    return Results.Conflict(new 
                    { 
                        message = "Email already exists."
                    });
                }

                userDb.Email = request.Email.Trim().ToLowerInvariant();
                userDb.DisplayName = request.DisplayName.Trim();

                await db.SaveChangesAsync();

                var response = new UserProfileResponse
                {
                    Id = userDb.Id,
                    Username = userDb.Username,
                    DisplayName = userDb.DisplayName,
                    Email = userDb.Email,
                    JoinedAt = userDb.JoinedAt
                };

                return Results.Ok(response);
            })
            .WithName("UpdateCurrentUser")
            .RequireAuthorization();
        }
    }
}
