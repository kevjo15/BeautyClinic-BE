﻿using Application_Layer.Jwt;
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
            services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            services.AddValidatorsFromAssembly(assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


            return services;
        }
    }
}
