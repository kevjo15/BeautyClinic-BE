using Domain_Layer.Models;

namespace Application_Layer.Commands.ServiceCommands.CreateService
{
    public class CreateServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ServiceModel CreatedService { get; set; }

        public static CreateServiceResult SuccessResult(string message, ServiceModel createdService) => 
            new CreateServiceResult { Success = true, Message = message, CreatedService = createdService };

        public static CreateServiceResult FailureResult(string message) => 
            new CreateServiceResult { Success = false, Message = message };
    }
} 