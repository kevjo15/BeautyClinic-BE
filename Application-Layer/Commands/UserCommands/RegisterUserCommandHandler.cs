using Domain_Layer.Models;
using Infrastructure_Layer.Repositories.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            if(request.NewUser == null)
            {
                throw new ArgumentNullException(nameof(request.NewUser), "Invalid user data. FirstName, LastName, Email, and Password are required.");
            }
            //if (request.NewUser.Password != request.NewUser.ConfirmPassword)
            //{
            //    throw new Exception("Passwords do not match.");
            //}
            try
            {
                var user = new UserModel
                {
                    UserName = request.NewUser.UserName,
                    Email = request.NewUser.Email,
                    FirstName = request.NewUser.FirstName,
                    LastName = request.NewUser.LastName
                };

                var result = await _userRepository.RegisterUserAsync(user, request.NewUser.Password);

                return result != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
