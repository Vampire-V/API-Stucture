using System.Text;
using Infrastructure.Services;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions;

public static class AuthenticationExtensions
{
    public static void ConfigureAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        // อ่านค่า JwtSettings จาก Configuration
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        if (jwtSettings == null)
        {
            throw new InvalidOperationException("JwtSettings configuration is missing.");
        }
        // โหลด Public Key จาก RsaKeyService
        var rsaKeyService = new RsaKeyService();
        rsaKeyService.EnsureKeysExist(); // สร้าง Key หากยังไม่มี
        var publicKey = rsaKeyService.LoadPublicKey();

        // เพิ่ม Authentication สำหรับ JWT
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new RsaSecurityKey(publicKey),
                };

                // ตั้งค่า Event สำหรับ JWT Authentication
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<
                            ILogger<JwtBearerEvents>
                        >();
                        logger.LogWarning(
                            "JWT authentication failed: {Exception}",
                            context.Exception
                        );
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<
                            ILogger<JwtBearerEvents>
                        >();
                        logger.LogInformation("JWT token validated.");
                        return Task.CompletedTask;
                    },
                };
            });
    }
}
