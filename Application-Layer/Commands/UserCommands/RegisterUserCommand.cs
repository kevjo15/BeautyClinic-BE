using Application_Layer.DTO_s;
using Domain_Layer.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands
{
    public class RegisterUserCommand : IRequest<bool>
    {
        public RegisterUserDTO NewUser { get; set; }

        public RegisterUserCommand(RegisterUserDTO newUser)
        {
            NewUser = newUser;
        }
    }
}
