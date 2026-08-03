// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PlatformHealthDashboard
//  Layer:          Infrastructure
//  File:           RequestMetricSample.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
//  SchemaVersion:  2026.1
//  Description:    Internal, Infrastructure-only transport DTO carried through the bounded
//                  capture channel between RequestLoggingMiddleware (via
//                  ChannelRequestMetricsRecorder) and RequestMetricsCaptureService. Column
//                  names match [PlatformHealth].[RequestMetricEvent] 1:1 for direct Dapper
//                  binding via usp_RequestMetricEvent_InsertBatch. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.PlatformHealthDashboard.Capture;

/// <summary>Single completed-HTTP-request sample queued for durable capture.</summary>
public sealed record RequestMetricSample
{
    /// <summary>Tenant the request was scoped to.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>The request's correlation identifier.</summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>HTTP method (GET, POST, ...).</summary>
    public required string Method { get; init; }

    /// <summary>Request path.</summary>
    public required string Path { get; init; }

    /// <summary>HTTP response status code.</summary>
    public required int StatusCode { get; init; }

    /// <summary>Elapsed request duration in milliseconds.</summary>
    public required long ElapsedMs { get; init; }

    /// <summary>UTC instant the request completed.</summary>
    public required DateTime OccurredAtUtc { get; init; }
}
