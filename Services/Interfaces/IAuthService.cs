using FamilyShoppingList.DTO;
using FamilyShoppingList.Models;

namespace FamilyShoppingList.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserRegisterResponse> Register(UserRegisterRequest request);

        Task<AuthTokenResponse> Login(UserLoginRequest request);

        Task<AuthTokenResponse> Refresh(string refreshToken);

        Task Logout(string refreshToken);

    }
}
