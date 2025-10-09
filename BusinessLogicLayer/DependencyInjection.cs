using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.Policies;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
        services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);
        services.AddScoped<ValidationService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IPolicyService, PolicyService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{Environment.GetEnvironmentVariable("REDIS_HOST")}:{Environment.GetEnvironmentVariable("REDIS_PORT")}";
        });

        // Add httpclient with base URI
        services.AddHttpClient<UserMicroserviceClient>(options =>
        {
            options.BaseAddress = new Uri($"http://{Environment.GetEnvironmentVariable("UserMicroserviceHost")}:{Environment.GetEnvironmentVariable("UserMicroservicePort")}");
        }).AddPolicyHandler(
            services.BuildServiceProvider().GetRequiredService<IPolicyService>()
                .UserServiceCombinedPolicy()
            );


        services.AddHttpClient<ProductMicroserviceClient>(options =>
        {
            options.BaseAddress = new Uri($"http://{Environment.GetEnvironmentVariable("ProductMicroserviceHost")}:{Environment.GetEnvironmentVariable("ProductMicroservicePort")}");
        }).AddPolicyHandler(
            services.BuildServiceProvider().GetRequiredService<IPolicyService>()
            .ProductServiceCombinedPolicy()
        );

        return services;
    }
}
