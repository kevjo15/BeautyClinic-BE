using Domain_Layer.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure_Layer.Repositories.User
{
    public interface IUserRepository
    {
        Task<bool> CheckPasswordAsync(UserModel user, string password);
        Task<UserModel> FindByEmailAsync(string email);
        Task<IdentityResult> RegisterUserAsync(UserModel newUser, string password);
    }
}