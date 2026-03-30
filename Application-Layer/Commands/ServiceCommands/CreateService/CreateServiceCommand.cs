using Application_Layer.DTO_s;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.ServiceCommands.CreateService
{
    public class CreateServiceCommand : IRequest<OperationResult<ServiceModel>>
    {
        public ServiceDTO ServiceDto { get; }

        public CreateServiceCommand(ServiceDTO ServiceModel)
        {
            ServiceDto = ServiceModel;
        }
    }
} 
