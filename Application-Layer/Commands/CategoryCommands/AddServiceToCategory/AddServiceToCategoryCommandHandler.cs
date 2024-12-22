using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure_Layer.Repositories.Service;
using Application_Layer.Commands.CategoryCommands.AddServiceToCategory;
using Application_Layer.Interfaces;

namespace Application_Layer.Commands.CategoryCommands.AddServiceToCategory
{
    public class AddServiceToCategoryCommandHandler : IRequestHandler<AddServiceToCategoryCommand, AddServiceToCategoryResult>
    {
        private readonly IServiceRepository _serviceRepository;

        public AddServiceToCategoryCommandHandler(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<AddServiceToCategoryResult> Handle(AddServiceToCategoryCommand request, CancellationToken cancellationToken)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
            if (service == null)
            {
                return new AddServiceToCategoryResult
                {
                    Success = false,
                    Message = "Service not found."
                };
            }

            service.CategoryId = request.CategoryId;
            var success = await _serviceRepository.UpdateServiceAsync(service);

            return new AddServiceToCategoryResult
            {
                Success = success,
                Message = success ? "Service linked to category successfully." : "Failed to link service to category."
            };
        }
    }
}