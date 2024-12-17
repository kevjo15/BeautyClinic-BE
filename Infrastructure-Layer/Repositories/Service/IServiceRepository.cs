using Domain_Layer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Repositories.Service
{
    public interface IServiceRepository
    {
        Task<IEnumerable<ServiceModel>> GetAllServicesAsync();
        Task<ServiceModel> GetServiceByIdAsync(Guid serviceId);
        Task<bool> AddServiceAsync(ServiceModel service);
        Task<bool> UpdateServiceAsync(ServiceModel service);
        Task DeleteServiceAsync(Guid serviceId);
        Task<IEnumerable<ServiceModel>> GetServicesByCategoryAsync(Guid categoryId);
    }
} 