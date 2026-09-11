namespace FamilyShoppingList.Exceptions;

public sealed class ForbiddenException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ForbiddenException(
        string message,
        IEnumerable<string>? errors = null)
        : base(message)
    {
        Errors = errors?.ToList() ?? [];
    }
}