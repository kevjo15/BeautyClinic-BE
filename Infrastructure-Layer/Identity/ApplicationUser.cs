using Microsoft.AspNetCore.Identity;

namespace Infrastructure_Layer.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>Blob-path till profilbilden ("&lt;container&gt;/&lt;blobNamn&gt;") — aldrig en färdig URL.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Stripe-customer-id för kort-på-fil. Null tills första kortet sparas.</summary>
    public string? StripeCustomerId { get; set; }
}
