using MediatR;
using Application_Layer.Interfaces;

namespace Application_Layer.Commands.CategoryCommands.RemoveServiceFromCategory
{
    public class RemoveServiceFromCategoryCommandHandler : IRequestHandler<RemoveServiceFromCategoryCommand, RemoveServiceFromCategoryResult>
    {
        private readonly IServiceRepository _serviceRepository;

        public RemoveServiceFromCategoryCommandHandler(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<RemoveServiceFromCategoryResult> Handle(RemoveServiceFromCategoryCommand request, CancellationToken cancellationToken)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
            if (service == null)
            {
                return new RemoveServiceFromCategoryResult
                {
                    Success = false,
                    Message = "Service not found."
                };
            }

            service.CategoryId = null; // Remove the category link
            var success = await _serviceRepository.UpdateServiceAsync(service);

            return new RemoveServiceFromCategoryResult
            {
                Success = success,
                Message = success ? "Service removed from category successfully." : "Failed to remove service from category."
            };
        }
    }
}