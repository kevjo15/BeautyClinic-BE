using Domain_Layer.Models;

namespace Infrastructure_Layer.Repositories.Service
{
    public interface IServiceRepository
    {
        Task<IEnumerable<ServiceModel>> GetAllServicesAsync();
        //Task<ServiceModel> GetServiceByIdAsync(Guid id);
        //Task<List<ServiceModel>> GetAllAsync();
        //Task<ServiceModel> GetByNameAsync(string name);
        Task<bool> AddServiceAsync(ServiceModel service);
        Task<bool> UpdateServiceAsync(ServiceModel service);
        Task DeleteServiceAsync(Guid id);
        Task<IEnumerable<ServiceModel>> GetServicesByCategoryAsync(Guid categoryId);
        Task<ServiceModel> GetServiceByIdAsync(Guid serviceId);
    }
}
