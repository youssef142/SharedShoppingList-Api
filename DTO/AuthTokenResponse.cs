namespace FamilyShoppingList.DTO
{
    public class AuthTokenResponse
    {
        public string AccessToken { get; init; } = string.Empty;

        public string RefreshToken { get; init; } = string.Empty;
    }
}
