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
[Route("groups/{groupId:guid}/categories")]
public class CategoriesController(ShoppingListDbContext db) : ControllerBase
{
    [HttpGet(Name = "GetCategories")]
    public async Task<IActionResult> GetCategories(Guid groupId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var isMember = await db.GroupMembers
            .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

        if (!isMember)
            return Forbid();

        var categories = await db.Categories
            .Where(c => c.GroupId == groupId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost(Name = "CreateCategory")]
    public async Task<IActionResult> CreateCategory(Guid groupId, CategoryCreateRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
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

        var isMember = await db.GroupMembers
            .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

        if (!isMember)
            return Forbid();

        var name = request.Name.Trim();

        var exists = await db.Categories
            .AnyAsync(c => c.GroupId == groupId &&
                           c.Name.ToLower() == name.ToLower());

        if (exists)
            return Conflict(new
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

        return Created($"/groups/{groupId}/categories/{category.Id}", response);
    }

    [HttpDelete("{categoryId:guid}", Name = "DeleteCategory")]
    public async Task<IActionResult> DeleteCategory(Guid groupId, Guid categoryId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var isMember = await db.GroupMembers
            .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

        if (!isMember)
            return Forbid();

        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.GroupId == groupId);

        if (category is null)
            return NotFound();

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
