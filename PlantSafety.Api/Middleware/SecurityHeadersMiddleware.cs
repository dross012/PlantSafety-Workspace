// ============================================================================
// File: SecurityHeadersMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: ASP.NET Core middleware that appends security-hardening HTTP
//              response headers to every response served by PlantSafety.Api.
// ============================================================================
//
// Onboarding Notes:
// - Registered as the first middleware in UsePlantSafetyMiddleware() so headers
//   are emitted on all responses, including errors from ExceptionHandlingMiddleware.
// - Headers injected:
//     X-Content-Type-Options: nosniff                    (MIME sniffing prevention)
//     X-Frame-Options: DENY                              (clickjacking prevention)
//     Referrer-Policy: strict-origin-when-cross-origin   (referrer information control)
//     X-XSS-Protection: 1; mode=block                   (legacy XSS filter activation)
//     Content-Security-Policy: default-src 'self'        (resource load restrictions)
//     Permissions-Policy: geolocation=(), camera=()      (feature policy)
// - Content-Security-Policy is intentionally restrictive. If the API serves
//   Swagger UI in development, the policy is relaxed to allow 'unsafe-inline'
//   for styles (required by Swashbuckle).
// - This middleware does NOT set HSTS; that is handled by the Azure Front Door
//   / Application Gateway layer in production and by UseHsts() in development.
//
// Rationale:
// - OWASP Top 10 A05 (Security Misconfiguration) mitigation. All security headers
//   are required by the PlantSafety.Infrastructure.Security zero-trust baseline.
//
// Future Extension Seams:
// - Add Strict-Transport-Security when the API moves behind its own domain (not
//   behind App Gateway which already terminates TLS).
// - Tighten CSP script-src once all static assets are on a CDN with known hashes.
//
// ============================================================================

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that appends security-hardening response headers to every HTTP response.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initialises a new instance of <see cref="SecurityHeadersMiddleware"/>.
    /// </summary>
    /// <param name="next">Next middleware delegate.</param>
    /// <param name="environment">Hosting environment used to relax CSP in Development.</param>
    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next        = next;
        _environment = environment;
    }

    /// <summary>Invokes the middleware and appends security headers before response flush.</summary>
    /// <param name="context">Current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            headers["X-Content-Type-Options"]  = "nosniff";
            headers["X-Frame-Options"]         = "DENY";
            headers["Referrer-Policy"]         = "strict-origin-when-cross-origin";
            headers["X-XSS-Protection"]        = "1; mode=block";
            headers["Permissions-Policy"]      = "geolocation=(), camera=(), microphone=()";

            var csp = _environment.IsDevelopment()
                ? "default-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:"
                : "default-src 'self'; img-src 'self' data:";
            headers["Content-Security-Policy"] = csp;

            return Task.CompletedTask;
        });

        await _next(context);
    }
}

/// <summary>Extension methods for registering <see cref="SecurityHeadersMiddleware"/>.</summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>Adds <see cref="SecurityHeadersMiddleware"/> to the request pipeline.</summary>
    /// <param name="builder">Application builder.</param>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        => builder.UseMiddleware<SecurityHeadersMiddleware>();
}
