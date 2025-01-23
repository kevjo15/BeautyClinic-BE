using Application_Layer.DTO_s;
using MediatR;

namespace Application_Layer.Commands.ServiceCommands.UpdateService
{
    public class UpdateServiceCommand : IRequest<UpdateServiceResult>
    {
        public Guid ServiceId { get; }
        public ServiceDTO ServiceDto { get; }

        public UpdateServiceCommand(Guid serviceId, ServiceDTO serviceDto)
        {
            ServiceId = serviceId;
            ServiceDto = serviceDto;
        }
    }
}