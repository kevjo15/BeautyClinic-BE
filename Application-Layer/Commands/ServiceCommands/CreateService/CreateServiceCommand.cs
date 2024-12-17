using Application_Layer.DTO_s;
using MediatR;

namespace Application_Layer.Commands.ServiceCommands.CreateService
{
    public class CreateServiceCommand : IRequest<CreateServiceResult>
    {
        public ServiceDTO ServiceDto { get; }

        public CreateServiceCommand(ServiceDTO serviceDto)
        {
            ServiceDto = serviceDto;
        }
    }
} 