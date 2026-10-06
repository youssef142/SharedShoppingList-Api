using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class InviteRequest
    {
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 50 characters.")]
        [RegularExpression(
            @"^[a-zA-Z\u0600-\u06FF0-9_-]+(?: [a-zA-Z\u0600-\u06FF0-9_-]+)*$",
            ErrorMessage = "Only Arabic/English letters, numbers, spaces, '-' and '_' are allowed."
        )]
        public string Name { get; set; }

        public required string Username { get; set; }
    }
}
