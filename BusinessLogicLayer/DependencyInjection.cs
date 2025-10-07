using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.Policies;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Polly;

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

        // Add httpclient with base URI
        services.AddHttpClient<UserMicroserviceClient>(options =>
        {
            options.BaseAddress = new Uri($"http://{Environment.GetEnvironmentVariable("UserMicroserviceHost")}:{Environment.GetEnvironmentVariable("UserMicroservicePort")}");
        }).AddPolicyHandler(
            services.BuildServiceProvider().GetRequiredService<IPolicyService>()
            .GetRetryPolicy(3, 2)
            );

        services.AddHttpClient<ProductMicroserviceClient>(options =>
        {
            options.BaseAddress = new Uri($"http://{Environment.GetEnvironmentVariable("ProductMicroserviceHost")}:{Environment.GetEnvironmentVariable("ProductMicroservicePort")}");
        });

        return services;
    }
}
