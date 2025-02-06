namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshTokenResult
    {
        public bool Successful { get; set; }
        public string Error { get; set; }
        public string AccessToken { get; set; }

        public RefreshTokenResult(bool successful, string error = null, string accessToken = null)
        {
            Successful = successful;
            Error = error;
            AccessToken = accessToken;
        }
    }
} 