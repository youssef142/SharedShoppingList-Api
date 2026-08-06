namespace FamilyShoppingList.Models
{
    public class GroupInvite
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Group? Group { get; set; }
        public Guid InvitedUserId { get; set; }
        public User? InvitedUser { get; set; }
        public Guid InvitingUserId { get; set; }
        public User? InvitingUser { get; set; }
        public InviteStatus InviteStatus { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
