using Application_Layer.Interfaces;
using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ServiceModel> GetServiceByIdAsync(Guid serviceId)
        {
            return await _context.Services.FindAsync(serviceId);
        }

        public async Task<bool> AddServiceAsync(ServiceModel service)
        {
            await _context.Services.AddAsync(service);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateServiceAsync(ServiceModel service)
        {
            _context.Services.Update(service);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task DeleteServiceAsync(Guid serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
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