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
[Route("groups/{groupId:guid}/Items")]
public class ItemsController (ShoppingListDbContext db) : ControllerBase
{
    [HttpGet(Name = "GetGroupItems")]
    public async Task<IActionResult> GetGroupItems(Guid groupId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var isMember = await db.GroupMembers.AnyAsync(gm =>
            gm.UserId == userId &&
            gm.GroupId == groupId);

        if (!isMember)
            return Forbid();

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

        return Ok(shoppingItems);
    }

    [HttpPost(Name = "CreateGroupItem")]
    public async Task<IActionResult> CreateGroupItem(Guid groupId, ShoppingItemCreateRequest request)
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

        var isMember = await db.GroupMembers.AnyAsync(gm =>
            gm.UserId == userId &&
            gm.GroupId == groupId);

        if (!isMember)
            return Forbid();

        Category? category = null;
        if (request.CategoryId is not null)
        {
            category = await db.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.GroupId == groupId);

            if (category is null)
                return BadRequest(new
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
            AddedByUsername = User.GetUsername(),
            CategoryId = category?.Id,
            CategoryName = category?.Name
        };
        return Created($"/groups/{groupId}/items/{item.Id}", response);
    }
}