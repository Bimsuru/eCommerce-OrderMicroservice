using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // Add mongodb into the service collection
        string connectionStirngTemplate = configuration.GetConnectionString("MongoDB")!;

        string connectionString = connectionStirngTemplate.Replace("$MONGO_HOST", Environment.GetEnvironmentVariable("MONGODB_HOST"))
                                                          .Replace("$MONGO_PORT", Environment.GetEnvironmentVariable("MONGODB_PORT"));

        // connect to the MongoDB and IMongoClient with connectionstring
        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

        // Then Connect Db with client and return the database for work with repository
        services.AddScoped<IMongoDatabase>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase("OrdersDatabase");

        });

        services.AddScoped<IOrderRepository, OrderRepository>();

    
        return services;
    }
}
