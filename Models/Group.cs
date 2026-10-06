namespace FamilyShoppingList.Models
{
    public class Group
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public Guid OwnerUserId { get; set; }
        public User? OwnerUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ShoppingItem> shoppingItems { get; set; }
            = new List<ShoppingItem>();
        public ICollection<GroupMember> Members { get; set; }
        = new List<GroupMember>();

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
