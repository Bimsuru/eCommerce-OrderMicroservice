namespace APILayer.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log exception type and message
            _logger.LogError($"{ex.GetType().ToString()}: {ex.Message}");

            // and check innerexception null or not
            if (ex.InnerException != null)
            {
                _logger.LogError($"{ex.InnerException.GetType().ToString()}: {ex.InnerException.Message}");
            }

            context.Response.StatusCode = 500; // Internal server error
            await context.Response.WriteAsJsonAsync(new { Message = ex.Message, Type = ex.GetType().ToString() }); // Write JSON body

        }
    }
}

// Extension method used to add the middleware to the http request pipeline
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
