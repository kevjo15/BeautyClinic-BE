using Domain_Layer.Models;

namespace Application_Layer.Commands.ServiceCommands.UpdateService
{
    public class UpdateServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ServiceModel UpdatedService { get; set; }

        public static UpdateServiceResult SuccessResult(string message, ServiceModel updatedService) => 
            new UpdateServiceResult { Success = true, Message = message, UpdatedService = updatedService };

        public static UpdateServiceResult FailureResult(string message) => 
            new UpdateServiceResult { Success = false, Message = message };
    }
} 