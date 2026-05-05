using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.ServiceCommands.DeleteService
{
    public class DeleteServiceCommand : IRequest<OperationResult>
    {
        public Guid ServiceId { get; }

        public DeleteServiceCommand(Guid serviceId)
        {
            ServiceId = serviceId;
        }
    }
}
