public class ItemEditRequestResponse
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public required string ItemName { get; set; }
    public required string RequestedByUsername { get; set; }
    public required string RequestType { get; set; }

    public Guid? PreviousCategoryId { get; set; }
    public string? PreviousCategoryName { get; set; }
    public Guid? ProposedCategoryId { get; set; }
    public string? ProposedCategoryName { get; set; }

    public string? PreviousStatus { get; set; }
    public string? ProposedStatus { get; set; }

    public Guid? PreviousAddedByUserId { get; set; }
    public string? PreviousAddedByUsername { get; set; }
    public Guid? ProposedAddedByUserId { get; set; }
    public string? ProposedAddedByUsername { get; set; }

    public Guid? PreviousStatusChangedByUserId { get; set; }
    public string? PreviousStatusChangedByUsername { get; set; }
    public Guid? ProposedStatusChangedByUserId { get; set; }
    public string? ProposedStatusChangedByUsername { get; set; }

    public required string RequestStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUsername { get; set; }
}