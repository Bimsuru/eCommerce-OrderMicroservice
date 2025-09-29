using APILayer.Middlewares;
using BusinessLogicLayer;
using BusinessLogicLayer.HttpClients;
using DataAccessLayer;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBusinessLogicLayer();
builder.Services.AddDataAccessLayer(builder.Configuration);


// Add Controllers to the service Collection
builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add cors enable
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Add httpclient with base URI
builder.Services.AddHttpClient<UserMicroserviceClient>(options =>
{
    options.BaseAddress = new Uri($"http://{Environment.GetEnvironmentVariable("UserMicroserviceHost")}:{Environment.GetEnvironmentVariable("UserMicroservicePort")}");
});

builder.Services.AddHttpClient<ProductMicroserviceClient>(options =>
{
    options.BaseAddress = new Uri($"http://{Environment.GetEnvironmentVariable("ProductMicroserviceHost")}:{Environment.GetEnvironmentVariable("ProductMicroservicePort")}");
});



var app = builder.Build();

// Configure the HTTP request pipeline.

// Middleware Added 
app.UseExceptionHandlingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Routing
app.UseRouting();

app.UseCors();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints after auth/authorization
app.MapControllers();

app.Run();

