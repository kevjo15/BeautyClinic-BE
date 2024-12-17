using Infrastructure_Layer.Repositories.Service;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Commands.ServiceCommands.DeleteService
{
    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, bool>
    {
        private readonly IServiceRepository _serviceRepository;

        public DeleteServiceCommandHandler(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<bool> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            await _serviceRepository.DeleteServiceAsync(request.ServiceId);
            return true;
        }
    }
} 