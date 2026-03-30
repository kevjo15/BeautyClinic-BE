using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Update
{
    public class UpdateUserProfileCommand : IRequest<OperationResult<UpdateUserProfileDTO>>
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
