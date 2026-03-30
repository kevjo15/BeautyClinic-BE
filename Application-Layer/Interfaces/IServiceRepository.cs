using Domain_Layer.Common;
using Domain_Layer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application_Layer.Interfaces
{
    public interface IServiceRepository
    {
        Task<IEnumerable<ServiceModel>> GetAllServicesAsync();
        Task<ServiceModel?> GetServiceByIdAsync(Guid serviceId);
        Task<OperationResult<ServiceModel>> AddServiceAsync(ServiceModel service);
        Task<OperationResult<ServiceModel>> UpdateServiceAsync(ServiceModel service);
        Task<OperationResult> DeleteServiceAsync(Guid id);
        Task<IEnumerable<ServiceModel>> GetServicesByCategoryAsync(Guid categoryId);
    }
}
