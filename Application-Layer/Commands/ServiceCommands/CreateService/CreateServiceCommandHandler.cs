using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.ServiceCommands.CreateService
{
    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, OperationResult<ServiceModel>>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IApplicationMapper _mapper;
        private readonly ILogger<CreateServiceCommandHandler> _logger;

        public CreateServiceCommandHandler(
            IServiceRepository serviceRepository,
            IApplicationMapper mapper,
            ILogger<CreateServiceCommandHandler> logger)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OperationResult<ServiceModel>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var service = _mapper.ToServiceModel(request.ServiceDto);
                service.Id = Guid.NewGuid();

                return await _serviceRepository.AddServiceAsync(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while creating service");
                return OperationResult<ServiceModel>.Failure("An unexpected error occurred.");
            }
        }
    }
}
