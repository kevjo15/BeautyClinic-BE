using Application_Layer.Interfaces;
using Application_Layer.Services;
using Azure.Communication.Email;
using Azure.Communication.Sms;
using Azure.Identity;
using Azure.Storage.Blobs;
using Infrastructure_Layer.DataSeeder;
using Infrastructure_Layer.Database;
using Infrastructure_Layer.Repositories;
using Infrastructure_Layer.Repositories.Conversation;
using Infrastructure_Layer.Repositories.Message;
using Infrastructure_Layer.Repositories.Notification;
using Infrastructure_Layer.Repositories.Service;
using Infrastructure_Layer.Repositories.User;
using Infrastructure_Layer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<DataSeeder.DataSeeder>();

            var useAzurite = configuration.GetValue<bool>("Storage:UseAzurite", false);
            if (useAzurite)
            {
                var conn = configuration["Storage:ConnectionString"] ?? "UseDevelopmentStorage=true";
                services.AddSingleton(new BlobServiceClient(conn));
                services.AddScoped<IFileService, FileService>();
            }
            else
            {
                var account = configuration["Storage:AccountName"];
                if (!string.IsNullOrEmpty(account))
                {
                    services.AddSingleton(new BlobServiceClient(new Uri($"https://{account}.blob.core.windows.net"), new DefaultAzureCredential()));
                    services.AddScoped<IFileService, FileService>();
                }
            }

            var communicationServicesConnection = configuration["CommunicationServices:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(communicationServicesConnection))
            {
                services.AddSingleton(new EmailClient(communicationServicesConnection));
                services.AddSingleton(new SmsClient(communicationServicesConnection));
                services.AddScoped<IEmailSender, AcsEmailSender>();
                services.AddScoped<ISmsSender, AcsSmsSender>();
            }
            else
            {
                services.AddScoped<IEmailSender, NullEmailSender>();
                services.AddScoped<ISmsSender, NullSmsSender>();
            }

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
