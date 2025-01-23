using Application_Layer.DTO_s;
using MediatR;

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
