using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace API_Layer.Middleware;

public static class RateLimitingExtensions
{
    public const string LoginPolicy = "login";
    public const string PasswordResetPolicy = "password-reset";

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(LoginPolicy, limiter =>
            {
                limiter.PermitLimit = 5;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiter.QueueLimit = 0;
            });

            // Stramare för lösenordsåterställning — varje anrop kan trigga ett mejl.
            options.AddFixedWindowLimiter(PasswordResetPolicy, limiter =>
            {
                limiter.PermitLimit = 3;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiter.QueueLimit = 0;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
