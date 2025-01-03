using Infrastructure_Layer.Database;
using Infrastructure_Layer.Repositories.User;
using Infrastructure_Layer.DataSeeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure_Layer.Repositories.Service;
using Application_Layer.Interfaces;
using Infrastructure_Layer.Repositories;
using Infrastructure_Layer.Repositories.Conversation;
using Application.Common.Interfaces;
using Infrastructure_Layer.Repositories.Message;


namespace Infrastructure_Layer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ElsaBeautyDbContext>(options =>
            options.UseSqlServer(connectionString));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<DataSeeder.DataSeeder>();

            return services;
        }

        public static async Task SeedDataAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder.DataSeeder>();
                await seeder.SeedAsync();
            }
        }
    }
}
