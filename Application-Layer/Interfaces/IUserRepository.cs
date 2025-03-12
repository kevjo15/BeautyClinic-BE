using Domain_Layer.Models;
using Microsoft.AspNetCore.Identity;

namespace Application_Layer.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> CheckPasswordAsync(UserModel user, string password);
        Task<UserModel> FindByEmailAsync(string email);
        Task<UserModel> FindByIdAsync(string userId);
        Task<IdentityResult> RegisterUserAsync(UserModel newUser, string password);
        Task<IdentityResult> UpdateUserAsync(UserModel user);
        Task<bool> RevokeRefreshTokenAsync(string userId);
        Task<IdentityResult> UpdatePasswordAsync(UserModel user, string newPassword);
    }
}