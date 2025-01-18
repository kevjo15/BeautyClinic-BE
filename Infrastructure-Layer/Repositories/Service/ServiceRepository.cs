using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Repositories.Service
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly ElsaBeautyDbContext _context;

        public ServiceRepository(ElsaBeautyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceModel>> GetAllServicesAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<ServiceModel> GetServiceByIdAsync(Guid id)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<ServiceModel>> GetAllAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<ServiceModel> GetByNameAsync(string name)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task<bool> AddServiceAsync(ServiceModel service)
        {
            await _context.Services.AddAsync(service);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        public async Task<bool> UpdateServiceAsync(ServiceModel service)
        {
            _context.Services.Update(service);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        public async Task DeleteServiceAsync(Guid id)
        {
            var service = await GetServiceByIdAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ServiceModel>> GetServicesByCategoryAsync(Guid categoryId)
        {
            return await _context.Services
                .Where(s => s.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}
