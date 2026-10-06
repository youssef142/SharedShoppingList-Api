using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class UserUpdateRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Display name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Display name must be between 1 and 50 characters.")]
                [RegularExpression(
            @"^[a-zA-Z\u0600-\u06FF0-9_-]+(?: [a-zA-Z\u0600-\u06FF0-9_-]+)*$",
            ErrorMessage = "Only Arabic/English letters, numbers, spaces, '-' and '_' are allowed."
        )]
        public string Name { get; set; }
        public string DisplayName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [RegularExpression(
            @"^[^<>]*$",
            ErrorMessage = "The value cannot contain '<' or '>'."
        )]
        [StringLength(50, MinimumLength = 2)]
        public string Email { get; set; } = string.Empty;
    }
}
