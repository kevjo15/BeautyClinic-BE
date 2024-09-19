using Application_Layer.DTO_s;
using Azure.Core;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands.Update
{
    public class UpdateUserProfileCommand : IRequest<UpdateUserProfileResult>
    {
        public string UserId { get; set; }

        public UpdateUserProfileDTO UpdatedProfileDTO { get; set; }
        public UpdateUserProfileCommand(string userId, UpdateUserProfileDTO updateUserProfileDTO)
        {
            UpdatedProfileDTO = updateUserProfileDTO;
            UserId = userId;
        }
    }
}
