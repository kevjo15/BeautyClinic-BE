using Microsoft.AspNetCore.Identity;

namespace Infrastructure_Layer.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsDeleted { get; set; }
}
