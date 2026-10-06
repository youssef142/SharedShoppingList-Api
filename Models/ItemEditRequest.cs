namespace FamilyShoppingList.Models
{
    public enum EditRequestType
    {
        Category,
        Status,
        AddedBy,          // correct who added the item
        StatusChangedBy   // correct who bought/cancelled it (status itself unchanged)
    }

    public enum EditRequestStatus
    {
        Pending,
        Approved,
        Declined
    }

    public class ItemEditRequest
    {
        public Guid Id { get; set; }

        public Guid ItemId { get; set; }
        public ShoppingItem? Item { get; set; }

        public Guid GroupId { get; set; }
        public Group? Group { get; set; }

        public Guid RequestedByUserId { get; set; }
        public User? RequestedByUser { get; set; }

        public EditRequestType RequestType { get; set; }

        // RequestType == Category
        public Guid? PreviousCategoryId { get; set; }
        public Guid? ProposedCategoryId { get; set; }

        // RequestType == Status
        public ItemStatus? PreviousStatus { get; set; }
        public ItemStatus? ProposedStatus { get; set; }

        // RequestType == AddedBy
        public Guid? PreviousAddedByUserId { get; set; }
        public Guid? ProposedAddedByUserId { get; set; }

        // RequestType == StatusChangedBy
        public Guid? PreviousStatusChangedByUserId { get; set; }
        public Guid? ProposedStatusChangedByUserId { get; set; }

        public EditRequestStatus RequestStatus { get; set; } = EditRequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public User? ReviewedByUser { get; set; }
    }
}
