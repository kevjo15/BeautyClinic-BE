using Application_Layer.DTO_s;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommand : IRequest<LoginResult>
    {
        public LoginUserDTO LoginUserDTO { get; private set; }
        public LoginCommand(LoginUserDTO loginUserDTO)
        {
            LoginUserDTO = loginUserDTO;
        }
    }
}
