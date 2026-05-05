namespace Application_Layer.DTOs
{
    public class UpdatePasswordDTO
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
} 
