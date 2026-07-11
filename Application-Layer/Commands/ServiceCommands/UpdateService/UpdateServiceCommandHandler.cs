using MediatR;
using Application_Layer.Mapping;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.ServiceCommands.UpdateService
{
    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, OperationResult<ServiceModel>>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IApplicationMapper _mapper;
        private readonly ILogger<UpdateServiceCommandHandler> _logger;

        public UpdateServiceCommandHandler(
            IServiceRepository serviceRepository,
            IApplicationMapper mapper,
            ILogger<UpdateServiceCommandHandler> logger)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OperationResult<ServiceModel>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingService = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
                if (existingService == null)
                {
                    return OperationResult<ServiceModel>.Failure(
                        "Service not found.", OperationFailureType.NotFound);
                }

                _mapper.UpdateServiceModel(request.ServiceDto, existingService);
                return await _serviceRepository.UpdateServiceAsync(existingService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while updating service");
                return OperationResult<ServiceModel>.Failure("An unexpected error occurred.");
            }
        }
    }
}
