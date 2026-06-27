// ============================================================================
// File: ApiVersionDeprecationMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: ASP.NET Core middleware that injects Deprecation and Sunset response
//              headers when the requested API version is in its sunset period.
// ============================================================================
//
// Onboarding Notes:
// - Registered in UsePlantSafetyMiddleware() after UseRouting (so the matched
//   route API version is available via IApiVersioningFeature) and before
//   MapControllers.
// - The deprecated version list and their sunset dates are configured in
//   ApiOptions:DeprecatedVersions (dictionary of "major.minor" -> sunset date).
// - Deprecation header format: RFC 7807 deprecation timestamp (ISO 8601).
// - Sunset header format: RFC 7231 HTTP-date (e.g. "Fri, 01 Jan 2027 00:00:00 GMT").
// - When no matching deprecation entry is found, the middleware is a no-op and
//   adds no headers.
//
// Rationale:
// - RFC 8594 (The Sunset HTTP Header Field) compliance, required for API
//   lifecycle governance. Gives API consumers machine-readable notice of
//   version sunsets without requiring documentation changes.
//
// Future Extension Seams:
// - Emit a platform.ApiVersionDeprecationWarned integration event for telemetry
//   dashboards tracking deprecated-version usage rates.
// - Add X-PlantSafety-Migration-Guide header with a docs URL for the replacement
//   version.
//
// ============================================================================

using Asp.Versioning;
using PlantSafety.Api.Configuration;
using PlantSafety.Api.Constants;

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that adds <c>Deprecation</c> and <c>Sunset</c> response headers when
/// the requested API version is in a configured sunset period.
/// </summary>
public sealed class ApiVersionDeprecationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiOptions      _options;

    /// <summary>
    /// Initialises a new instance of <see cref="ApiVersionDeprecationMiddleware"/>.
    /// </summary>
    public ApiVersionDeprecationMiddleware(RequestDelegate next, IOptions<ApiOptions> options)
    {
        _next    = next;
        _options = options.Value;
    }

    /// <summary>
    /// Injects deprecation headers if the resolved API version is configured as deprecated.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        var versionFeature = context.Features.Get<IApiVersioningFeature>();
        var requestedVersion = versionFeature?.RequestedApiVersion?.ToString();

        if (requestedVersion is null)
            return;

        if (_options.DeprecatedVersions.TryGetValue(requestedVersion, out var sunsetDate))
        {
            context.Response.Headers[ApiHeaderConstants.Deprecation] =
                DateTimeOffset.UtcNow.ToString("r");
            context.Response.Headers[ApiHeaderConstants.Sunset] =
                sunsetDate.ToString("r");
        }
    }
}

/// <summary>Extension methods for registering <see cref="ApiVersionDeprecationMiddleware"/>.</summary>
public static class ApiVersionDeprecationMiddlewareExtensions
{
    /// <summary>Adds <see cref="ApiVersionDeprecationMiddleware"/> to the pipeline.</summary>
    public static IApplicationBuilder UseApiVersionDeprecation(this IApplicationBuilder builder)
        => builder.UseMiddleware<ApiVersionDeprecationMiddleware>();
}
