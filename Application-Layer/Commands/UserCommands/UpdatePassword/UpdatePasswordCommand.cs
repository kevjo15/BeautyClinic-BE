using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.UpdatePassword
{
    public class UpdatePasswordCommand : IRequest<OperationResult>
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
