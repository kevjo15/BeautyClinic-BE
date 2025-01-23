using Application_Layer.Interfaces;
using AutoMapper;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.ServiceCommands.CreateService
{
    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, CreateServiceResult>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public CreateServiceCommandHandler(IServiceRepository serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<CreateServiceResult> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var service = _mapper.Map<ServiceModel>(request.ServiceDto);
                service.Id = Guid.NewGuid();

                await _serviceRepository.AddServiceAsync(service);
                return CreateServiceResult.SuccessResult("Service created successfully.", service);
            }
            catch (Exception ex)
            {
                return CreateServiceResult.FailureResult($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}