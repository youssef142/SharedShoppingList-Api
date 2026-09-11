namespace FamilyShoppingList.Exceptions;

public sealed class UnauthorizedException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public UnauthorizedException(
        string message,
        IEnumerable<string>? errors = null)
        : base(message)
    {
        Errors = errors?.ToList() ?? [];
    }
}