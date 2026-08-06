using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class InviteRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
        [RegularExpression(
            @"^[a-zA-Z0-9_-]+$",
            ErrorMessage = "Username can only contain letters, numbers, '-' and '_'."
        )]
        public required string Username { get; set; }
    }
}
