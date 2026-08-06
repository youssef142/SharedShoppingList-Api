namespace FamilyShoppingList.DTO
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
