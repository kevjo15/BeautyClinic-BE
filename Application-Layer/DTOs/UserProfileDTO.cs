namespace Application_Layer.DTOs
{
    public class UserProfileDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        /// <summary>Färdigsignerad läs-URL till profilbilden (aldrig rå blob-path).</summary>
        public string? AvatarUrl { get; set; }
    }
}
