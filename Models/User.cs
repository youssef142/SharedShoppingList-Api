namespace FamilyShoppingList.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public List<ShoppingItem>? ItemsAdded { get; set; }
        public List<ShoppingItem>? ItemsStatusChanged { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();
        public ICollection<GroupMember> GroupMemberships { get; set; }
        = new List<GroupMember>();
        public ICollection<Group> OwnedGroups { get; set; }
        = new List<Group>();

    }
}
