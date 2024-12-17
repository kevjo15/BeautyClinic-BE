using Infrastructure_Layer.Repositories.Service;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;

namespace Application_Layer.Commands.ServiceCommands.UpdateService
{
    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, UpdateServiceResult>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public UpdateServiceCommandHandler(IServiceRepository serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<UpdateServiceResult> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingService = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
                if (existingService == null)
                {
                    return UpdateServiceResult.FailureResult("Service not found.");
                }

                _mapper.Map(request.ServiceDto, existingService);
                await _serviceRepository.UpdateServiceAsync(existingService);
                return UpdateServiceResult.SuccessResult("Service updated successfully.", existingService);
            }
            catch (Exception ex)
            {
                return UpdateServiceResult.FailureResult($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
} 