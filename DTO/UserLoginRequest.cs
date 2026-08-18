using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class UserLoginRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
        [RegularExpression(
            @"^[a-zA-Z0-9_-]+$",
            ErrorMessage = "Username can only contain letters, numbers, hyphens, and underscores."
        )]
        public required string Username { get; set; }


        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(
            @"^(?=.*[A-Za-z])(?=.*\d)\S{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain at least one letter, one number, and no spaces."
        )]
        public required string Password { get; set; }
    }
}
