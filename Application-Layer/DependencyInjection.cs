using Application_Layer.Common;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using Application_Layer.PipelineBehaviour;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application_Layer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;
            services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));

            services.AddValidatorsFromAssembly(assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            services.AddScoped<IServiceImageUrlResolver, ServiceImageUrlResolver>();

            // Mapperly-mappern är stateless och trådsäker → singleton.
            services.AddSingleton<IApplicationMapper, ApplicationMapper>();

            return services;
        }
    }
}
