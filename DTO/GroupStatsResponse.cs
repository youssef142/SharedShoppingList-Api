namespace FamilyShoppingList.DTO
{
    public class GroupStatsResponse
    {
        public int TotalItems { get; set; }
        public int TotalBought { get; set; }
        public int TotalPending { get; set; }
        public int TotalCancelled { get; set; }
        public List<UserStats> ByUser { get; set; } = new();
        public List<ProductStats> TopProducts { get; set; } = new();
        public List<CategoryStats> TopCategories { get; set; } = new();
    }
}
