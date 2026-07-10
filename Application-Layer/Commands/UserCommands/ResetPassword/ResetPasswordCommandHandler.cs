using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, OperationResult>
    {
        private readonly IUserRepository _userRepository;

        public ResetPasswordCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<OperationResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Request;
            return await _userRepository.ResetPasswordWithTokenAsync(
                dto.Email.Trim(),
                dto.Token,
                dto.NewPassword);
        }
    }
}
