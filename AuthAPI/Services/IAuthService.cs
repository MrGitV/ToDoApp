using AuthAPI.Models;

namespace AuthAPI.Services
{
    public interface IAuthService
    {
        Task<TokenResponse> Login(LoginModel model);
        Task<bool> Register(RegisterModel model);
        Task<bool> UserExists(string username);
    }
}