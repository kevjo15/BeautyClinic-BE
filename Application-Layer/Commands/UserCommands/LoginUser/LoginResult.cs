using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
