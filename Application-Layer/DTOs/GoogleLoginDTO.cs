namespace Application_Layer.DTOs
{
    /// <summary>Body för POST /api/auth/google — ID-tokenet från Google Identity Services.</summary>
    public class GoogleLoginDTO
    {
        public string IdToken { get; set; } = string.Empty;
    }
}
