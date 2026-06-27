// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           CorrelationIdMiddleware.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    ASP.NET Core middleware that reads or generates X-Correlation-Id headers and propagates them.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.AspNetCore.Http;

namespace PlantSafety.Infrastructure.Common.Middleware;

/// <summary>
/// ASP.NET Core middleware that reads the <c>X-Correlation-Id</c> request header
/// or generates a new UUIDv7 when absent, then propagates it on the response.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string Header = "X-Correlation-Id";

    /// <inheritdoc cref="RequestDelegate"/>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(Header, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Uuid7.New().ToString("D");

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[Header] = correlationId;

        await next(context);
    }
}
