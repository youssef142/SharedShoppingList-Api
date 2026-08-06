namespace FamilyShoppingList.DTO
{
    public class GroupWithOwnerNameResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OwnerUsername { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
