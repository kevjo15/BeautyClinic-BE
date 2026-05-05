using Domain_Layer.Common;
using Domain_Layer.Models;

namespace Application_Layer.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> CheckPasswordAsync(UserModel user, string password);
        Task<UserModel?> FindByEmailAsync(string email);
        Task<UserModel?> FindByIdAsync(string userId);
        Task<OperationResult> RegisterUserAsync(UserModel newUser, string password);
        Task<OperationResult> UpdateUserAsync(UserModel user);
        Task<OperationResult> UpdatePasswordAsync(UserModel user, string newPassword);
        Task<UserModel?> GetFirstEmployeeAsync();
        Task<List<UserModel>> GetEmployeesAsync();
        Task<IList<string>> GetRolesAsync(UserModel user);
    }
}
