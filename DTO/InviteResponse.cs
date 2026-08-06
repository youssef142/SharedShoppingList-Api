using FamilyShoppingList.Models;

namespace FamilyShoppingList.DTO
{
    public class InviteResponse
    {
        public Guid Id { get; set; }

        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string? InvitedUsername { get; set; }
        public string? InvitingUsername { get; set; }
        public InviteStatus InviteStatus { get; set; }
        public DateTime CreatedAt { get; set; } 
    }
}
