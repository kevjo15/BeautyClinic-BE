using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, OperationResult>
    {
        private readonly IUserRepository _userRepository;

        public ConfirmEmailCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<OperationResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            return await _userRepository.ConfirmEmailAsync(request.Request.UserId, request.Request.Token);
        }
    }
}
