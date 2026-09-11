using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class UserLoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(
            @"^[A-Za-z0-9.@]+$",
            ErrorMessage = "Email can contain only letters, numbers, '.', and '@'."
        )]
                [EmailAddress(ErrorMessage = "Invalid email address.")]
                [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Email must be between 3 and 100 characters."
        )]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(
            @"^(?=.*[A-Za-z])(?=.*\d)\S{8,}$",
            ErrorMessage = "Password must contain at least one letter, one number, and no spaces."
        )]
        public required string Password { get; set; }
    }
}
