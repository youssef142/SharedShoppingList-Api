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
        [StringLength(200, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 200 characters.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9<>]).*$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character. Characters '<' and '>' are not allowed."
        )]
        public required string Password { get; set; }
    }
}
