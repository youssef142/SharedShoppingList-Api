namespace FamilyShoppingList.DTO
{
    public class ItemSuggestionResponse
    {
        public required string Name { get; set; }
        public int TimesAdded { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
