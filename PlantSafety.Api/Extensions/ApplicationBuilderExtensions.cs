// ============================================================================
// File: ApplicationBuilderExtensions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Extension methods for configuring the ASP.NET Core middleware pipeline
//              in the canonical order required by the PlantSafety Architecture Specification.
// ============================================================================
//
// Onboarding Notes:
// - Called from Program.cs: app.UsePlantSafetyMiddleware().
// - The middleware ORDER is architecturally mandated (Architecture Specification §10.4.2 Rule 4):
//   1. CorrelationIdMiddleware     — must be first to attach CorrelationId before any logging
//   2. ExceptionHandlingMiddleware — catches all unhandled exceptions before they escape
//   3. RequestLoggingMiddleware    — measures total request duration including auth
//   4. TenantResolutionMiddleware  — resolves tenant from authenticated JWT claims
//   5. UseRouting()
//   6. UseCors()
//   7. UseAuthentication()         — validates JWT Bearer token
//   8. UseAuthorization()          — enforces policy-based access control
//   9. UseOutputCache()
//   10. MapControllers / MapHealthChecks — endpoint execution
// - DO NOT add Serilog request logging (UseSerilogRequestLogging) here — that is in
//   Infrastructure's pipeline setup. This file only configures Presentation-layer middleware.
//
// Rationale:
// - Enforcing the canonical middleware order as a single extension method prevents
//   individual developers from accidentally misording security-critical middleware.
//
// Future Extension Seams:
// - Add UseResponseCompression() for large payload endpoints (Analytics reporting).
// - Add UseRequestDecompression() to support compressed POST payloads from IoT devices.
//
// ============================================================================

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using PlantSafety.Api.Middleware;

namespace PlantSafety.Api.Extensions;

/// <summary>
/// Extension methods for configuring the PlantSafety.Api ASP.NET Core middleware pipeline
/// in the architecturally mandated order.
/// </summary>
public static class ApplicationBuilderExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Configures the PlantSafety.Api middleware pipeline in canonical order.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    /// <returns>The <see cref="WebApplication"/> for chaining.</returns>
    public static WebApplication UsePlantSafetyMiddleware(this WebApplication app)
    {
        // 1. Correlation ID — must be first
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 2. Global exception handling — before logging to catch all exceptions
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // 3. Request logging
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 4. Routing (required before auth middleware)
        app.UseRouting();

        // 5. CORS
        app.UseCors("PlantSafetyDefault");

        // 6. Authentication (JWT validation)
        app.UseAuthentication();

        // 7. Authorization (policy enforcement)
        app.UseAuthorization();

        // 8. Tenant resolution — after authentication, JWT must be validated first
        app.UseMiddleware<TenantResolutionMiddleware>();

        // 9. Output cache
        app.UseOutputCache();

        return app;
    }

    /// <summary>
    /// Maps all health check endpoints with the standard PlantSafety response format.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    /// <returns>The <see cref="WebApplication"/> for chaining.</returns>
    public static WebApplication MapPlantSafetyHealthChecks(this WebApplication app)
    {
        // Overall health (liveness + readiness)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        }).AllowAnonymous();

        // Readiness probe — DB, Redis, Service Bus
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        }).AllowAnonymous();

        // Liveness probe — process is running
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthCheckResponse
        }).AllowAnonymous();

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    status = kvp.Value.Status.ToString(),
                    description = kvp.Value.Description,
                    duration = kvp.Value.Duration.TotalMilliseconds,
                    tags = kvp.Value.Tags
                })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
