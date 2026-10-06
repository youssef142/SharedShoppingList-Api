using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using FamilyShoppingList.Extensions;
using FamilyShoppingList.Services.Interfaces;


namespace FamilyShoppingList.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {

            app.MapPost("/register", async (UserRegisterRequest request, ShoppingListDbContext db, IAuthService authService) =>
            {
                var errors = request.ValidateModel();

                if (errors is not null)
                {
                    return Results.BadRequest(new
                    {
                        message = "Validation failed.",
                        errors
                    });
                }

                var responseUser = await authService.Register(request);
                return Results.Created($"/user/{responseUser.Id}", responseUser);

            })
            .WithName("Register");

            app.MapPost("/login", async (UserLoginRequest request, HttpContext http, IAuthService authService) =>
            {
                var errors = request.ValidateModel();

                if (errors is not null)
                {
                    return Results.BadRequest(new
                    {
                        message = "Validation failed.",
                        errors
                    });
                }

                var loginResponse = await authService.Login(request);

                http.SetRefreshToken(loginResponse.RefreshToken);

                return Results.Ok(new
                {
                    accessToken = loginResponse.AccessToken
                });
            })
            .WithName("Login");

            app.MapPost("/refresh", async (
                HttpContext http,
                IAuthService authService) =>
            {
                var refreshToken = http.Request.Cookies["refreshToken"];
                foreach (var cookie in http.Request.Cookies)
                {
                    Console.WriteLine($"{cookie.Key} = {cookie.Value}");
                }
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return Results.BadRequest(new
                    {
                        message = "Refresh failed.",
                        errors = new[] { "Refresh token is missing." }
                    });

                var result = await authService.Refresh(refreshToken);

                http.SetRefreshToken(result.RefreshToken);

                return Results.Ok(new
                {
                    accessToken = result.AccessToken
                });
            })
            .WithName("RefreshToken");

            app.MapPost("/logout", async (
                HttpContext http,
                IAuthService authService) =>
            {
                var refreshToken = http.Request.Cookies["refreshToken"];

                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    await authService.Logout(refreshToken);
                }

                http.Response.Cookies.Delete("refreshToken");

                return Results.NoContent();
            })
            .WithName("Logout");
        }
    }
}
