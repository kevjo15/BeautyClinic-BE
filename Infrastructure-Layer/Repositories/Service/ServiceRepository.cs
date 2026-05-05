using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public async Task<ServiceModel?> GetServiceByIdAsync(Guid serviceId)
        {
            return await _context.Services.FindAsync(serviceId);
        }

        public async Task<OperationResult<ServiceModel>> AddServiceAsync(ServiceModel service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return OperationResult<ServiceModel>.Success(service);
        }

        public async Task<OperationResult<ServiceModel>> UpdateServiceAsync(ServiceModel service)
        {
            var existingService = await _context.Services.FindAsync(service.Id);
            if (existingService == null)
            {
                return OperationResult<ServiceModel>.Failure("Service not found.");
            }

            _context.Services.Update(service);
            await _context.SaveChangesAsync();
            return OperationResult<ServiceModel>.Success(service);
        }

        public async Task<OperationResult> DeleteServiceAsync(Guid id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return OperationResult.Failure("Service not found.");
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return OperationResult.Success();
        }

        public async Task<IEnumerable<ServiceModel>> GetServicesByCategoryAsync(Guid categoryId)
        {
            return await _context.Services.Where(s => s.CategoryId == categoryId).ToListAsync();
        }

        public async Task<IReadOnlyList<ServiceModel>> GetAllAsync(CancellationToken ct)
        {
            return await _context.Services.AsNoTracking().ToListAsync(ct);
        }

        public async Task<ServiceModel?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.Services.FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task UpdateAsync(ServiceModel service, CancellationToken ct)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync(ct);
        }
    }
}
