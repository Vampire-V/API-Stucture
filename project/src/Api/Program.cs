using Api.Extensions;
using Api.Middleware;
using Application.Interfaces;
using Application.Workflows;
using Domain.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.UnitOfWork;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

// Configure Extensions
builder.Services.ConfigureSwagger();
builder.Services.ConfigureCors(builder.Configuration);
builder.Services.ConfigureAuthentication(builder.Configuration, builder.Environment);
builder.Services.ConfigureRateLimiting();

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("Infrastructure")
    );
});

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Check if running during design-time (for migrations)
var isDesignTime = args.Contains("--isDesignTime");
Console.WriteLine($"Running in Design Time: {isDesignTime}");

if (!isDesignTime)
{
    // Register Services
    builder.Services.AddScoped<IUserWorkflow, UserWorkflow>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRoleRepository, RoleRepository>();
    // Register JwtTokenValidator as a singleton
    builder.Services.AddSingleton<JwtTokenValidator>(provider =>
    {
        var jwtSettings = provider.GetRequiredService<IOptions<JwtSettings>>().Value;
        return new JwtTokenValidator(jwtSettings);
    });
    // Add Rsa
    builder.Services.AddSingleton<RsaKeyService>();
}

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!isDesignTime)
{
    app.UseHttpsRedirection();
    app.UseRateLimiter(); // Add Rate Limiter Middleware
    app.UseCors("DefaultPolicy"); // Add CORS Middleware
    app.Use(
        async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            await next();
        }
    );
    app.UseMiddleware<RsaInitializerMiddleware>();
    app.UseAuthentication(); // Add Authentication Middleware
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();
}

await app.RunAsync();
