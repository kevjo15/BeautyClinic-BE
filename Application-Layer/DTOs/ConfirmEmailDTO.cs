namespace Application_Layer.DTOs
{
    public class ConfirmEmailDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class ResendConfirmationDTO
    {
        public string Email { get; set; } = string.Empty;
    }
}
