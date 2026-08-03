// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.Tracing
//  Layer:          Infrastructure
//  File:           TracingServiceRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
//  SchemaVersion:  2026.1
//  Description:    Real OpenTelemetry distributed-tracing wiring (Handbook Phase 6 §5.1
//                  Distributed Tracing). Previously OpenTelemetry.Extensions.Hosting was
//                  referenced in PlantSafety.Infrastructure.csproj but NO AddOpenTelemetry()/
//                  WithTracing() call existed anywhere in the solution -- a dead package
//                  reference. ASP.NET Core's built-in Activity infrastructure already populates
//                  Activity.Current (which OperationContextAccessor already reads for
//                  TraceId/SpanId) even without this wiring, but with no TracerProvider
//                  registered those spans were never exported, sampled consistently, or extended
//                  to outbound HttpClient/SqlClient calls -- so no real END-TO-END distributed
//                  trace ever existed across service/database boundaries. This file closes that
//                  gap. Additive-only evolution.
// ============================================================================
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PlantSafety.Infrastructure.Observability.Tracing;

/// <summary>
/// Registers OpenTelemetry distributed tracing: ASP.NET Core, HttpClient, and SqlClient
/// instrumentation, exported via the console (always, for local/dev visibility) and via OTLP
/// when <c>"OpenTelemetry:OtlpEndpoint"</c> is configured (opt-in, no hardcoded collector).
/// </summary>
public static class TracingServiceRegistration
{
    /// <summary>Adds real OpenTelemetry tracing instrumentation and exporters.</summary>
    public static IServiceCollection AddPlantSafetyTracing(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: "PlantSafety.Api",
                serviceVersion: typeof(TracingServiceRegistration).Assembly.GetName().Version?.ToString() ?? "unknown"))
            .WithTracing(builder =>
            {
                builder
                    // Covers every inbound ASP.NET Core request -- the exact spans
                    // OperationContextAccessor.TraceId/SpanId already read from Activity.Current.
                    .AddAspNetCoreInstrumentation()
                    // Covers every outbound HttpClient call (cross-service integrations).
                    .AddHttpClientInstrumentation()
                    // Covers the underlying SqlClient connections Dapper issues. Dapper itself has
                    // no ActivitySource, but Microsoft.Data.SqlClient's own diagnostics does. Left
                    // at library defaults deliberately: recent OpenTelemetry.Instrumentation.
                    // SqlClient versions do NOT capture raw SQL statement text in span attributes
                    // by default, satisfying Handbook §2.23 Redaction & Sanitization Rule
                    // ("Remove SQL text") without any extra configuration.
                    .AddSqlClientInstrumentation()
                    // Custom ActivitySources declared across the platform's Diagnostics/
                    // Telemetry constant files (e.g. BehaviorDiagnostics.ActivitySourceName).
                    .AddSource("PlantSafety.*")
                    .AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    builder.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return services;
    }
}
