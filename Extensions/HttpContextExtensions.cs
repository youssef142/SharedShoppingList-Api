namespace FamilyShoppingList.Extensions
{
    public static class HttpContextExtensions
    {
        public static void SetRefreshToken(this HttpContext http, string token)
        {
            http.Response.Cookies.Append(
                "refreshToken",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
        }

        public static void DeleteRefreshToken(this HttpContext http)
        {
            http.Response.Cookies.Delete("refreshToken");
        }
    }
}
