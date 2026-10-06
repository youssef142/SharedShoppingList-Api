namespace FamilyShoppingList.DTO
{
    public class CreateEditRequestRequest
    {
        public required string RequestType { get; set; }   // "Category" or "Status"
        public Guid? ProposedCategoryId { get; set; }       // used if RequestType == Category (null allowed = "remove category")
        public string? ProposedStatus { get; set; }         // "Pending" | "Bought" | "Cancelled", used if RequestType == Status
        public Guid? ProposedUserId { get; set; }           // used if RequestType == "AddedBy" or "StatusChangedBy"
    }
}
