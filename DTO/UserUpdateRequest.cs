using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class UserUpdateRequest
    {
        [Required(ErrorMessage = "DisplayName is required.")]
        [StringLength(30, MinimumLength = 2)]
        [RegularExpression(
            @"^[a-zA-Z ]+$",
            ErrorMessage = "Display name can only contain letters and spaces."
        )]
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
