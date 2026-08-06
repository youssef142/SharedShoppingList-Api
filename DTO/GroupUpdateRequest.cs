using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class GroupUpdateRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Group name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Group name must be between 1 and 50 characters.")]
        [RegularExpression(
            @"^[a-zA-Z0-9 _-]+$",
            ErrorMessage = "Only letters, numbers, spaces, '-' and '_' are allowed."
        )]
        public string Name { get; set; }
    }
}
