namespace FamilyShoppingList.Exceptions;

public sealed class NotFoundException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public NotFoundException(
        string message,
        IEnumerable<string>? errors = null)
        : base(message)
    {
        Errors = errors?.ToList() ?? [];
    }
}