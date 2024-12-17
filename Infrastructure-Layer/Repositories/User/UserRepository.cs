using Domain_Layer.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure_Layer.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;

        public UserRepository(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<bool> CheckPasswordAsync(UserModel user, string password)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return result.Succeeded;
        }

        public async Task<UserModel> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<UserModel> FindByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<UserModel> FindByRefreshTokenAsync(string refreshToken)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(user => user.RefreshToken == refreshToken);
        }

        public async Task<IdentityResult> RegisterUserAsync(UserModel newUser, string password)
        {
            var result = await _userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                // Assign default role
                await _userManager.AddToRoleAsync(newUser, "Customer");
            }
            return result;
        }

        public async Task<IdentityResult> UpdateUserAsync(UserModel user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            return true;
        }

        public async Task<IdentityResult> UpdatePasswordAsync(UserModel user, string newPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }
    }
}
