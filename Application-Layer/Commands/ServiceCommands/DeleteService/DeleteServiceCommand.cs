using MediatR;
using System;

namespace Application_Layer.Commands.ServiceCommands.DeleteService
{
    public class DeleteServiceCommand : IRequest<bool>
    {
        public Guid ServiceId { get; }

        public DeleteServiceCommand(Guid serviceId)
        {
            ServiceId = serviceId;
        }
    }
} 