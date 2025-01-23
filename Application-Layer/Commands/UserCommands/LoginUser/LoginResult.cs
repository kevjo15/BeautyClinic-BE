namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginResult
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? Error { get; set; }
        public bool Successful { get; set; }
    }
}
