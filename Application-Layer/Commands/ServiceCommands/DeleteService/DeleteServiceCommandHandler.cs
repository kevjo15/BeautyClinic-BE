using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.ServiceCommands.DeleteService
{
    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, OperationResult>
    {
        private readonly IServiceRepository _serviceRepository;

        public DeleteServiceCommandHandler(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<OperationResult> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            return await _serviceRepository.DeleteServiceAsync(request.ServiceId);
        }
    }
}
