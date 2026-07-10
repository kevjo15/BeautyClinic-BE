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

        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            return identityUser == null
                ? null
                : await _userManager.GeneratePasswordResetTokenAsync(identityUser);
        }

        public async Task<OperationResult> ResetPasswordWithTokenAsync(string email, string token, string newPassword)
        {
            const string invalidLinkMessage = "Länken är ogiltig eller har gått ut. Begär en ny återställningslänk.";

            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser == null)
            {
                // Avslöja inte om adressen finns — samma fel som för ogiltig token.
                return OperationResult.Failure(invalidLinkMessage);
            }

            var result = await _userManager.ResetPasswordAsync(identityUser, token, newPassword);
            if (result.Succeeded)
            {
                return OperationResult.Success();
            }

            return result.Errors.Any(e => e.Code == "InvalidToken")
                ? OperationResult.Failure(invalidLinkMessage)
                : ToOperationResult(result);
        }

        public async Task<(string UserId, string Token)?> GenerateEmailConfirmationTokenAsync(string email)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser == null || identityUser.EmailConfirmed)
            {
                return null;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
            return (identityUser.Id, token);
        }

        public async Task<OperationResult> ConfirmEmailAsync(string userId, string token)
        {
            const string invalidLinkMessage = "Länken är ogiltig eller har gått ut. Begär ett nytt bekräftelsemejl.";

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
            {
                return OperationResult.Failure(invalidLinkMessage);
            }

            if (identityUser.EmailConfirmed)
            {
                // Redan bekräftad — t.ex. dubbelklick på länken. Behandla som lyckat.
                return OperationResult.Success();
            }

            var result = await _userManager.ConfirmEmailAsync(identityUser, token);
            if (result.Succeeded)
            {
                return OperationResult.Success();
            }

            return result.Errors.Any(e => e.Code == "InvalidToken")
                ? OperationResult.Failure(invalidLinkMessage)
                : ToOperationResult(result);
        }

        public async Task<OperationResult> AnonymizeAndDeactivateAsync(string userId)
        {
            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
            {
                return OperationResult.Failure("User not found.", OperationFailureType.NotFound);
            }

            // Nolla personuppgifter.
            identityUser.FirstName = "Borttagen";
            identityUser.LastName = "användare";
            identityUser.PhoneNumber = null;
            identityUser.AvatarUrl = null;
            identityUser.IsDeleted = true;

            // Tombstone frigör den riktiga e-posten/användarnamnet så de kan
            // återregistreras. SetEmail/SetUserName normaliserar och persisterar.
            var tombstone = $"deleted-{identityUser.Id}";
            var emailResult = await _userManager.SetEmailAsync(identityUser, $"{tombstone}@deleted.local");
            if (!emailResult.Succeeded) return ToOperationResult(emailResult);

            var nameResult = await _userManager.SetUserNameAsync(identityUser, tombstone);
            if (!nameResult.Succeeded) return ToOperationResult(nameResult);

            // Spärra inloggning: ta bort lösenordet och rotera security-stampen.
            if (await _userManager.HasPasswordAsync(identityUser))
            {
                var pwdResult = await _userManager.RemovePasswordAsync(identityUser);
                if (!pwdResult.Succeeded) return ToOperationResult(pwdResult);
            }

            // Koppla bort externa logins (Google) — annars kan det raderade kontot
            // fortfarande hittas via provider-nyckeln och loggas in igen.
            foreach (var login in await _userManager.GetLoginsAsync(identityUser))
            {
                await _userManager.RemoveLoginAsync(identityUser, login.LoginProvider, login.ProviderKey);
            }

            await _userManager.UpdateSecurityStampAsync(identityUser);

            // Persistera de nollade scalar-fälten (Set*-anropen ovan sparade bara sina egna).
            var updateResult = await _userManager.UpdateAsync(identityUser);
            return ToOperationResult(updateResult);
        }

        public async Task<UserModel?> FindByExternalLoginAsync(string provider, string providerKey)
        {
            var identityUser = await _userManager.FindByLoginAsync(provider, providerKey);
            return identityUser == null ? null : MapToDomainUser(identityUser);
        }

        public async Task<OperationResult> AddExternalLoginAsync(string userId, string provider, string providerKey)
        {
            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
            {
                return OperationResult.Failure("User not found.", OperationFailureType.NotFound);
            }

            var result = await _userManager.AddLoginAsync(
                identityUser, new UserLoginInfo(provider, providerKey, provider));
            return ToOperationResult(result);
        }

        public async Task<OperationResult> RegisterExternalUserAsync(UserModel newUser, string provider, string providerKey)
        {
            var identityUser = new ApplicationUser();
            MapToIdentityUser(newUser, identityUser);

            // Leverantören (Google) har redan verifierat e-postadressen.
            identityUser.EmailConfirmed = true;

            // Inget lösenord — den externa leverantören äger autentiseringen.
            var createResult = await _userManager.CreateAsync(identityUser);
            if (!createResult.Succeeded)
            {
                return ToOperationResult(createResult);
            }

            var roleResult = await _userManager.AddToRoleAsync(identityUser, "Customer");
            if (!roleResult.Succeeded)
            {
                return ToOperationResult(roleResult);
            }

            var loginResult = await _userManager.AddLoginAsync(
                identityUser, new UserLoginInfo(provider, providerKey, provider));
            if (!loginResult.Succeeded)
            {
                return ToOperationResult(loginResult);
            }

            newUser.Id = identityUser.Id;
            newUser.UserName = identityUser.UserName;
            newUser.EmailConfirmed = true;
            return OperationResult.Success();
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
                IsDeleted = user.IsDeleted,
                AvatarUrl = user.AvatarUrl,
                EmailConfirmed = user.EmailConfirmed
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
            target.AvatarUrl = source.AvatarUrl;
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
