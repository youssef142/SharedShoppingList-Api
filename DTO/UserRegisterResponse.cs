using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.DTO
{
    public class UserRegisterResponse
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}
