using Application_Layer.DTO_s;
using MediatR;

namespace Application_Layer.Commands.UserCommands.UpdatePassword
{
    public class UpdatePasswordCommand : IRequest<bool>
    {
        public string UserId { get; }
        public UpdatePasswordDTO UpdatePasswordDTO { get; }

        public UpdatePasswordCommand(string userId, UpdatePasswordDTO updatePasswordDTO)
        {
            UserId = userId;
            UpdatePasswordDTO = updatePasswordDTO;
        }
    }
} 