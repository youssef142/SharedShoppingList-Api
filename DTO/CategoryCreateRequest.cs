using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class CategoryCreateRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Category name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 50 characters.")]
        [RegularExpression(
            @"^[a-zA-Z\u0600-\u06FF0-9_-]+(?: [a-zA-Z\u0600-\u06FF0-9_-]+)*$",
            ErrorMessage = "Only Arabic/English letters, numbers, spaces, '-' and '_' are allowed."
        )]
        public string Name { get; set; }
    }
}
