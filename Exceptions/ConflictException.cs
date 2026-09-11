namespace FamilyShoppingList.Exceptions;

public sealed class ConflictException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ConflictException(
        string message,
        IEnumerable<string>? errors = null)
        : base(message)
    {
        Errors = errors?.ToList() ?? [];
    }
}