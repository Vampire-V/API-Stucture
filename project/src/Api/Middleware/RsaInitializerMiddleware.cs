using Infrastructure.Services;

namespace Api.Middleware;

public class RsaInitializerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RsaKeyService _rsaKeyService;

    public RsaInitializerMiddleware(RequestDelegate next, RsaKeyService rsaKeyService)
    {
        _next = next;
        _rsaKeyService = rsaKeyService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _rsaKeyService.EnsureKeysExist();
        await _next(context);
    }
}
