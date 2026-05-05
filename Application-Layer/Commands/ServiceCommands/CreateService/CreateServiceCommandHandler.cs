using Application_Layer.Interfaces;
using AutoMapper;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.ServiceCommands.CreateService
{
    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, OperationResult<ServiceModel>>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public CreateServiceCommandHandler(IServiceRepository serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<ServiceModel>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var service = _mapper.Map<ServiceModel>(request.ServiceDto);
                service.Id = Guid.NewGuid();

                return await _serviceRepository.AddServiceAsync(service);
            }
            catch (Exception ex)
            {
                return OperationResult<ServiceModel>.Failure($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
