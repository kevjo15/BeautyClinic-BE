namespace Domain_Layer.Models
{
    public class UserModel
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsDeleted { get; set; }
        public string? AvatarUrl { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
