// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PlatformHealthDashboard
//  Layer:          Infrastructure
//  File:           ChannelRequestMetricsRecorder.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
//  SchemaVersion:  2026.1
//  Description:    Non-blocking IRequestMetricsRecorder implementation. Every HTTP request
//                  writes one sample into a bounded, single-writer/single-reader in-memory
//                  channel (mirrors the OutboxProcessor polling-batch pattern, but push-based
//                  via Channel<T> rather than poll-based over a SQL table, since these samples
//                  do not need outbox-grade transactional durability). RequestMetricsCaptureService
//                  drains the channel and bulk-inserts into [PlatformHealth].[RequestMetricEvent].
//                  BoundedChannelFullMode.DropWrite: under sustained overload, the dashboard may
//                  under-sample rather than ever blocking or faulting the HTTP request pipeline --
//                  metrics capture must never become a performance liability itself.
//                  Additive-only evolution.
// ============================================================================
using System.Threading.Channels;
using PlantSafety.Application.Abstractions.Observability;

namespace PlantSafety.Infrastructure.Analytics.PlatformHealthDashboard.Capture;

/// <summary>
/// Bounded-channel-backed <see cref="IRequestMetricsRecorder"/>. Registered as a singleton and
/// shared with <see cref="RequestMetricsCaptureService"/>, which drains and persists samples.
/// </summary>
public sealed class ChannelRequestMetricsRecorder : IRequestMetricsRecorder
{
    private const int ChannelCapacity = 10_000;

    private readonly Channel<RequestMetricSample> _channel = Channel.CreateBounded<RequestMetricSample>(
        new BoundedChannelOptions(ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        });

    /// <summary>The channel reader consumed by <see cref="RequestMetricsCaptureService"/>.</summary>
    internal ChannelReader<RequestMetricSample> Reader => _channel.Reader;

    /// <inheritdoc/>
    public void Record(string method, string path, int statusCode, long elapsedMs, Guid correlationId, Guid tenantId)
    {
        // TryWrite never throws and never blocks -- if the channel is full (sustained overload),
        // the sample is dropped per BoundedChannelFullMode.DropWrite above. Best-effort by design.
        _channel.Writer.TryWrite(new RequestMetricSample
        {
            TenantId = tenantId,
            CorrelationId = correlationId,
            Method = method,
            Path = path.Length > 512 ? path[..512] : path,
            StatusCode = statusCode,
            ElapsedMs = elapsedMs,
            OccurredAtUtc = DateTime.UtcNow
        });
    }
}
