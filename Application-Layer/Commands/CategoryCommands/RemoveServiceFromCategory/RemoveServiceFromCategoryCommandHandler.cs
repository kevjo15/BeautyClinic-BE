using MediatR;
using Application_Layer.Interfaces;
using Domain_Layer.Common;

namespace Application_Layer.Commands.CategoryCommands.RemoveServiceFromCategory
{
    public class RemoveServiceFromCategoryCommandHandler : IRequestHandler<RemoveServiceFromCategoryCommand, OperationResult>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ICategoryRepository _categoryRepository;

        public RemoveServiceFromCategoryCommandHandler(IServiceRepository serviceRepository, ICategoryRepository categoryRepository)
        {
            _serviceRepository = serviceRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<OperationResult> Handle(RemoveServiceFromCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
            {
                return OperationResult.Failure("Category not found.");
            }

            var service = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
            if (service == null)
            {
                return OperationResult.Failure("Service not found.");
            }

            if (service.CategoryId != request.CategoryId)
            {
                return OperationResult.Failure("Service is not linked to the specified category.");
            }

            service.CategoryId = null;
            var result = await _serviceRepository.UpdateServiceAsync(service);
            return result.Successful
                ? OperationResult.Success()
                : OperationResult.Failure(result.Error ?? "Failed to remove service from category.");
        }
    }
}
