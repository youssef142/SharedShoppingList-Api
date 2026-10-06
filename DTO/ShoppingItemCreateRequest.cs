using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class ShoppingItemCreateRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "item name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "item name must be between 1 and 50 characters.")]
        [RegularExpression(
            @"^[a-zA-Z\u0600-\u06FF0-9_-]+(?: [a-zA-Z\u0600-\u06FF0-9_-]+)*$",
            ErrorMessage = "Only Arabic/English letters, numbers, spaces, '-' and '_' are allowed."
        )]
        
        public required string Name { get; set; }


        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
        public int Quantity { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
