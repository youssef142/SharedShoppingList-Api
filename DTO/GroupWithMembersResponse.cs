using FamilyShoppingList.Models;

namespace FamilyShoppingList.DTO
{
    public class GroupWithMembersResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OwnerUsername { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<UserRegisterResponse> Members { get; set; }
        = new List<UserRegisterResponse>();

    }
}
