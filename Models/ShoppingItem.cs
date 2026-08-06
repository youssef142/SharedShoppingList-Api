
using FamilyShoppingList.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class ShoppingItem
{
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    public required string Name { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive.")]
    public int Quantity { get; set; }

    public DateTime AddedDate { get; set; }
    public ItemStatus Status { get; set; }
    public DateTime? StatusDate { get; set; }

    public Guid? AddedByUserId { get; set; }
    public User? AddedByUser { get; set; }
    public Guid? StatusChangedByUserId { get; set; }
    public User? StatusChangedByUser { get; set; }
    public Guid GroupId { get; set; }
    public Group? Group { get; set; }
}

