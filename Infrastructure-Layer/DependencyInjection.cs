using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using Microsoft.Extensions.Logging;
using Azure.Communication.Email;
using Azure.Communication.Sms;
using Azure.Storage.Blobs;
using Infrastructure_Layer.DataSeeder;
using Infrastructure_Layer.Database;
using Infrastructure_Layer.Identity;
using Infrastructure_Layer.Repositories;
using Infrastructure_Layer.Repositories.Schedule;
using Infrastructure_Layer.Repositories.WorkDay;
using Infrastructure_Layer.Repositories.Conversation;
using Infrastructure_Layer.Repositories.Message;
using Infrastructure_Layer.Repositories.Notification;
using Infrastructure_Layer.Repositories.RefreshToken;
using Infrastructure_Layer.Repositories.Service;
using Infrastructure_Layer.Repositories.User;
using Infrastructure_Layer.Services;
using Infrastructure_Layer.Storage;
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
            services.AddScoped<IEmployeeScheduleRepository, ScheduleRepository>();
            services.AddScoped<IEmployeeWorkDayRepository, WorkDayRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IGoogleIdTokenValidator, GoogleIdTokenValidator>();
            services.AddScoped<DataSeeder.DataSeeder>();

            services.AddMemoryCache();

            var storageConnectionString = configuration["Storage:ConnectionString"];
            if (!string.IsNullOrEmpty(storageConnectionString))
            {
                services.AddSingleton(new BlobServiceClient(storageConnectionString));
                services.AddScoped<IFileService, AzureBlobFileService>();
            }
            else
            {
                services.AddScoped<IFileService, NullFileService>();
            }

            services.AddScoped<IPasswordResetEmailService, Notifications.PasswordResetEmailService>();
            services.AddScoped<IEmailConfirmationEmailService, Notifications.EmailConfirmationEmailService>();

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

            var stripeSecretKey = configuration["Stripe:SecretKey"];
            if (!string.IsNullOrWhiteSpace(stripeSecretKey))
            {
                var stripeWebhookSecret = configuration["Stripe:WebhookSecret"];
                services.AddScoped<IStripePaymentService>(sp => new Services.StripePaymentService(
                    stripeSecretKey,
                    stripeWebhookSecret,
                    sp.GetRequiredService<ILogger<Services.StripePaymentService>>()));
            }
            else
            {
                services.AddScoped<IStripePaymentService, Services.NullStripePaymentService>();
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
