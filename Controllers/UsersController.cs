using FamilyShoppingList.DTO;
using FamilyShoppingList.Data;
using FamilyShoppingList.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyShoppingList.Controllers;

[ApiController]
[Authorize]
[Route("Users")]
public class UsersController(ShoppingListDbContext db) : ControllerBase
{
    [HttpGet("me", Name = "GetCurrentUser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.GetUserId();

        if (userId == null)
            return Unauthorized();

        var userDb = await db.Users.FindAsync(userId);

        if (userDb == null)
            return NotFound();

        var response = new UserProfileResponse
        {
            Id = userDb.Id,
            Username = userDb.Username,
            DisplayName = userDb.DisplayName,
            Email = userDb.Email,
            JoinedAt = userDb.JoinedAt
        };

        return Ok(response);
    }

    [HttpPatch("me", Name = "UpdateCurrentUser")]
    public async Task<IActionResult> UpdateCurrentUser(UserUpdateRequest request)
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

        var userDb = await db.Users.FindAsync(userId);

        if (userDb == null)
            return NotFound();

        var exists = await db.Users
            .AnyAsync(x => x.Email == request.Email && x.Id != userId);

        if (exists)
        {
            return Conflict(new
            {
                message = "Registration failed.",
                errors = new[] { "Email already exists." }
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

        return Ok(response);
    }
}
