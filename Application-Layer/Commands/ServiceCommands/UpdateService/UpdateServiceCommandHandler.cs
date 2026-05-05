using MediatR;
using AutoMapper;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;

namespace Application_Layer.Commands.ServiceCommands.UpdateService
{
    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, OperationResult<ServiceModel>>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public UpdateServiceCommandHandler(IServiceRepository serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<ServiceModel>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingService = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
                if (existingService == null)
                {
                    return OperationResult<ServiceModel>.Failure("Service not found.");
                }

                _mapper.Map(request.ServiceDto, existingService);
                return await _serviceRepository.UpdateServiceAsync(existingService);
            }
            catch (Exception ex)
            {
                return OperationResult<ServiceModel>.Failure($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
