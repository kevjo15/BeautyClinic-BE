using Microsoft.AspNetCore.Identity;

namespace Domain_Layer.Models
{
    public class UserModel : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsDeleted { get; set; }
    }
}
