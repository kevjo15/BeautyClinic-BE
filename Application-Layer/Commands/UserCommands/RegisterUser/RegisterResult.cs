using Domain_Layer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands.RegisterUser
{
    public class RegisterResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
        public UserModel CreatedUser { get; set; }

        public RegisterResult(bool success, UserModel createdUser = null, List<string> errors = null)
        {
            Success = success;
            CreatedUser = createdUser;
            Errors = errors ?? new List<string>();
        }
    }
}
