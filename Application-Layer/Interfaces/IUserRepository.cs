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
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<OperationResult> ResetPasswordWithTokenAsync(string email, string token, string newPassword);
        /// <summary>Returnerar null om användaren saknas eller redan är bekräftad.</summary>
        Task<(string UserId, string Token)?> GenerateEmailConfirmationTokenAsync(string email);
        Task<OperationResult> ConfirmEmailAsync(string userId, string token);

        /// <summary>
        /// GDPR-radering: anonymiserar personuppgifterna, frigör e-post/användarnamn
        /// (tombstone), sätter IsDeleted, tar bort lösenordet, kopplar bort externa
        /// logins (Google) och roterar security-stampen. Användarraden behålls så
        /// refererande bokningar (FK Restrict) inte bryts.
        /// </summary>
        Task<OperationResult> AnonymizeAndDeactivateAsync(string userId);

        // Externa logins (AspNetUserLogins), t.ex. provider "Google" + Googles sub som nyckel.
        Task<UserModel?> FindByExternalLoginAsync(string provider, string providerKey);
        Task<OperationResult> AddExternalLoginAsync(string userId, string provider, string providerKey);

        /// <summary>
        /// Skapar en användare utan lösenord (extern identitetsleverantör äger
        /// autentiseringen): EmailConfirmed sätts direkt (leverantören har verifierat
        /// e-posten), Customer-roll, och den externa loginen kopplas.
        /// </summary>
        Task<OperationResult> RegisterExternalUserAsync(UserModel newUser, string provider, string providerKey);
        /// <summary>Sparar Stripe-customer-id på användaren (kort-på-fil). Rör inga andra fält.</summary>
        Task<OperationResult> SetStripeCustomerIdAsync(string userId, string stripeCustomerId);
        Task<UserModel?> GetFirstEmployeeAsync();
        Task<List<UserModel>> GetEmployeesAsync();
        Task<IList<string>> GetRolesAsync(UserModel user);
    }
}
