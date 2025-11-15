using AuthAPI.Models;

namespace AuthAPI.Services
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginModel model);
        Task<bool> RegisterAsync(RegisterModel model);
        Task<bool> UserExistsAsync(string username);
    }
}