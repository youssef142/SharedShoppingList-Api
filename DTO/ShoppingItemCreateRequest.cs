using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class ShoppingItemCreateRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Item name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Item name must be between 1 and 100 characters.")]
        [RegularExpression(
            @"^[a-zA-Z0-9 _-]+$",
            ErrorMessage = "Only letters, numbers, spaces, '-' and '_' are allowed."
        )]
        public required string Name { get; set; }


        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
        public int Quantity { get; set; }
    }
}
