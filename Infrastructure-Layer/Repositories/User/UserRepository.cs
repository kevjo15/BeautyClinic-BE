using Domain_Layer.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<UserModel> _userManager;

        public UserRepository(UserManager<UserModel> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserModel> RegisterUserAsync(UserModel newUser, string password)
        {
            var result = await _userManager.CreateAsync(newUser, password);
            return newUser;
        }
    }
}
