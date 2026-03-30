using MediatR;
using Application_Layer.Interfaces;
using Domain_Layer.Common;

namespace Application_Layer.Commands.CategoryCommands.AddServiceToCategory
{
    public class AddServiceToCategoryCommandHandler : IRequestHandler<AddServiceToCategoryCommand, OperationResult>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ICategoryRepository _categoryRepository;

        public AddServiceToCategoryCommandHandler(IServiceRepository serviceRepository, ICategoryRepository categoryRepository)
        {
            _serviceRepository = serviceRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<OperationResult> Handle(AddServiceToCategoryCommand request, CancellationToken cancellationToken)
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

            service.CategoryId = request.CategoryId;
            var result = await _serviceRepository.UpdateServiceAsync(service);
            return result.Successful
                ? OperationResult.Success()
                : OperationResult.Failure(result.Error ?? "Failed to link service to category.");
        }
    }
}
