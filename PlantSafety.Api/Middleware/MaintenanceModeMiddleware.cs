// ============================================================================
// File: MaintenanceModeMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: ASP.NET Core middleware that returns HTTP 503 Service Unavailable
//              for all non-health-check requests when the API is in maintenance mode.
// ============================================================================
//
// Onboarding Notes:
// - Maintenance mode is controlled by ApiOptions:MaintenanceMode (bool). Toggle it
//   via an Azure App Configuration / Key Vault feature flag without redeployment.
// - Health-check endpoints (/health, /health/ready, /health/live) are always
//   allowed through so that AKS readiness/liveness probes continue to function
//   during planned maintenance windows.
// - The 503 response body is RFC 7807 ProblemDetails to maintain a consistent
//   error envelope for all clients.
// - Registered early in UsePlantSafetyMiddleware() (after SecurityHeaders, before
//   ExceptionHandling) so maintenance mode takes priority over all other processing.
//
// Rationale:
// - Enables zero-downtime maintenance by gracefully shedding traffic to a 503
//   rather than allowing partial failures during schema migrations or deployments.
//
// Future Extension Seams:
// - Add Retry-After header with planned maintenance end time from ApiOptions.
// - Add bypass via a maintenance token header for internal health checks that
//   need to exercise the full pipeline during maintenance.
//
// ============================================================================

using PlantSafety.Api.Configuration;
using PlantSafety.Api.Constants;

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that short-circuits all non-health-check requests with HTTP 503
/// when <see cref="ApiOptions.MaintenanceMode"/> is <c>true</c>.
/// </summary>
public sealed class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiOptions      _options;

    private static readonly HashSet<string> HealthPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        ApiRouteConstants.Health,
        ApiRouteConstants.HealthReady,
        ApiRouteConstants.HealthLive
    };

    /// <summary>
    /// Initialises a new instance of <see cref="MaintenanceModeMiddleware"/>.
    /// </summary>
    public MaintenanceModeMiddleware(RequestDelegate next, IOptions<ApiOptions> options)
    {
        _next    = next;
        _options = options.Value;
    }

    /// <summary>
    /// Checks maintenance mode and either short-circuits with 503 or delegates to the next middleware.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (_options.MaintenanceMode && !HealthPaths.Contains(context.Request.Path.Value ?? string.Empty))
        {
            context.Response.StatusCode  = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new
            {
                type     = "https://tools.ietf.org/html/rfc7807",
                title    = "Service Unavailable",
                status   = 503,
                detail   = "PlantSafety API is currently undergoing maintenance. Please try again later.",
                instance = context.Request.Path.Value
            });
            return;
        }

        await _next(context);
    }
}

/// <summary>Extension methods for registering <see cref="MaintenanceModeMiddleware"/>.</summary>
public static class MaintenanceModeMiddlewareExtensions
{
    /// <summary>Adds <see cref="MaintenanceModeMiddleware"/> to the pipeline.</summary>
    public static IApplicationBuilder UseMaintenanceMode(this IApplicationBuilder builder)
        => builder.UseMiddleware<MaintenanceModeMiddleware>();
}
