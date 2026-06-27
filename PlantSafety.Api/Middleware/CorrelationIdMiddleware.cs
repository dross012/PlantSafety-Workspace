// ============================================================================
// File: CorrelationIdMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: ASP.NET Core middleware that propagates the X-Correlation-ID header
//              across all service boundaries for end-to-end distributed tracing.
// ============================================================================
//
// Onboarding Notes:
// - Must be registered FIRST in the middleware pipeline (before TenantResolutionMiddleware).
//   See Program.cs pipeline registration and Architecture Specification §4.10.
// - If the inbound request carries an X-Correlation-ID header, it is reused; otherwise
//   a new GUID is generated and attached to both the request and response.
// - The resolved CorrelationId is stored in HttpContext.Items["CorrelationId"] for
//   downstream middleware, controllers, and MediatR handlers to retrieve.
// - All Serilog log enrichment with CorrelationId is handled in Infrastructure
//   (LogContext.PushProperty), NOT here. This middleware only resolves and propagates.
//
// Rationale:
// - Correlation ID propagation is a Zero-Trust, observability-first architectural requirement.
//   Every cross-service request must be traceable from gateway to handler to storage.
//
// Future Extension Seams:
// - Support W3C TraceParent propagation alongside X-Correlation-ID.
// - Add validation that inbound correlation IDs conform to the expected UUID format.
//
// ============================================================================

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that reads or generates a correlation ID for each HTTP request
/// and propagates it through the request pipeline and the X-Correlation-ID response header.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    /// <summary>The HTTP header name used to propagate the correlation ID.</summary>
    public const string HeaderName = "X-Correlation-ID";

    /// <summary>The key used to store the correlation ID in <see cref="HttpContext.Items"/>.</summary>
    public const string ItemsKey = "CorrelationId";

    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of <see cref="CorrelationIdMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Resolves or generates the correlation ID for the current HTTP request,
    /// stores it in <see cref="HttpContext.Items"/>, and echoes it in the response header.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Items[ItemsKey] = correlationId;
        context.Response.Headers.TryAdd(HeaderName, correlationId);

        await _next(context);
    }
}
