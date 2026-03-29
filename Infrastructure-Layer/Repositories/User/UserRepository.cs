using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using Infrastructure_Layer.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure_Layer.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<bool> CheckPasswordAsync(UserModel user, string password)
        {
            var identityUser = await FindIdentityUserAsync(user);
            if (identityUser == null)
            {
                return false;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(identityUser, password, false);
            return result.Succeeded;
        }

        public async Task<UserModel?> FindByEmailAsync(string email)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            return identityUser == null ? null : MapToDomainUser(identityUser);
        }

        public async Task<UserModel?> FindByIdAsync(string userId)
        {
            var identityUser = await _userManager.FindByIdAsync(userId);
            return identityUser == null ? null : MapToDomainUser(identityUser);
        }

        public async Task<OperationResult> RegisterUserAsync(UserModel newUser, string password)
        {
            var identityUser = new ApplicationUser();
            MapToIdentityUser(newUser, identityUser);

            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                return ToOperationResult(result);
            }

            var roleResult = await _userManager.AddToRoleAsync(identityUser, "Customer");
            if (!roleResult.Succeeded)
            {
                return ToOperationResult(roleResult);
            }

            newUser.Id = identityUser.Id;
            newUser.UserName = identityUser.UserName;
            return OperationResult.Success();
        }

        public async Task<OperationResult> UpdateUserAsync(UserModel user)
        {
            var identityUser = await FindIdentityUserAsync(user);
            if (identityUser == null)
            {
                return OperationResult.Failure("User not found.");
            }

            MapToIdentityUser(user, identityUser);
            var result = await _userManager.UpdateAsync(identityUser);
            return ToOperationResult(result);
        }

        public async Task<OperationResult> UpdatePasswordAsync(UserModel user, string newPassword)
        {
            var identityUser = await FindIdentityUserAsync(user);
            if (identityUser == null)
            {
                return OperationResult.Failure("User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
            var result = await _userManager.ResetPasswordAsync(identityUser, token, newPassword);
            return ToOperationResult(result);
        }

        public async Task<UserModel?> GetFirstEmployeeAsync()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var employee = employees.FirstOrDefault();
            return employee == null ? null : MapToDomainUser(employee);
        }

        public async Task<List<UserModel>> GetEmployeesAsync()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            return employees.Select(MapToDomainUser).ToList();
        }

        public async Task<IList<string>> GetRolesAsync(UserModel user)
        {
            var identityUser = await FindIdentityUserAsync(user);
            return identityUser == null
                ? Array.Empty<string>()
                : await _userManager.GetRolesAsync(identityUser);
        }

        private async Task<ApplicationUser?> FindIdentityUserAsync(UserModel user)
        {
            if (!string.IsNullOrWhiteSpace(user.Id))
            {
                return await _userManager.FindByIdAsync(user.Id);
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                return await _userManager.FindByEmailAsync(user.Email);
            }

            if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                return await _userManager.FindByNameAsync(user.UserName);
            }

            return null;
        }

        private static UserModel MapToDomainUser(ApplicationUser user)
        {
            return new UserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsDeleted = user.IsDeleted
            };
        }

        private static void MapToIdentityUser(UserModel source, ApplicationUser target)
        {
            target.UserName = source.UserName;
            target.Email = source.Email;
            target.PhoneNumber = source.PhoneNumber;
            target.FirstName = source.FirstName;
            target.LastName = source.LastName;
            target.IsDeleted = source.IsDeleted;
        }

        private static OperationResult ToOperationResult(IdentityResult result)
        {
            if (result.Succeeded)
            {
                return OperationResult.Success();
            }

            var error = string.Join(" ", result.Errors.Select(e => e.Description));
            return OperationResult.Failure(string.IsNullOrWhiteSpace(error) ? "Identity operation failed." : error);
        }
    }
}
