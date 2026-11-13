using FamilyShoppingList.DTO;
using FamilyShoppingList.Extensions;
using FamilyShoppingList.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FamilyShoppingList.Controllers;

[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register", Name = "Register")]
    public async Task<IActionResult> Register(UserRegisterRequest request)
    {
        var errors = request.ValidateModel();

        if (errors is not null)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors
            });
        }

        var responseUser = await authService.Register(request);
        return Created($"/user/{responseUser.Id}", responseUser);
    }

    [HttpPost("login", Name = "Login")]
    public async Task<IActionResult> Login(UserLoginRequest request)
    {
        var errors = request.ValidateModel();

        if (errors is not null)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors
            });
        }

        var loginResponse = await authService.Login(request);

        HttpContext.SetRefreshToken(loginResponse.RefreshToken);

        return Ok(new
        {
            accessToken = loginResponse.AccessToken
        });
    }

    [HttpPost("refresh", Name = "RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = HttpContext.Request.Cookies["refreshToken"];
        foreach (var cookie in HttpContext.Request.Cookies)
        {
            Console.WriteLine($"{cookie.Key} = {cookie.Value}");
        }
        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(new
            {
                message = "Refresh failed.",
                errors = new[] { "Refresh token is missing." }
            });

        var result = await authService.Refresh(refreshToken);

        HttpContext.SetRefreshToken(result.RefreshToken);

        return Ok(new
        {
            accessToken = result.AccessToken
        });
    }

    [HttpPost("logout", Name = "Logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = HttpContext.Request.Cookies["refreshToken"];

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await authService.Logout(refreshToken);
        }

        HttpContext.Response.Cookies.Delete("refreshToken");

        return NoContent();
    }
}
