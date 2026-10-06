namespace FamilyShoppingList.DTO
{
    public class ShoppingItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; }
        public string Status { get; set; }
        public DateTime? StatusDate { get; set; }
        public string? AddedByUsername { get; set; }

        public string? StatusChangedByUsername { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }

    }
}
