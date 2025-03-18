using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions;

public static class CorsExtensions
{
    public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins =
            configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(
                "DefaultPolicy",
                policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            );
        });
    }
}
