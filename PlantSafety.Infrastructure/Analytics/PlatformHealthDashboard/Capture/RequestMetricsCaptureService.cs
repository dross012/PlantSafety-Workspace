// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PlatformHealthDashboard
//  Layer:          Infrastructure
//  File:           RequestMetricsCaptureService.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
//  SchemaVersion:  2026.1
//  Description:    Hosted service that periodically drains ChannelRequestMetricsRecorder's
//                  bounded channel and bulk-inserts the batch into
//                  [PlatformHealth].[RequestMetricEvent] via usp_RequestMetricEvent_InsertBatch.
//                  Mirrors OutboxProcessor's singleton-hosted-service + ISqlConnectionFactory
//                  pattern (both are safe to consume from a singleton because
//                  SqlConnectionFactory is itself stateless and opens a dedicated connection per
//                  call). Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Hosting;

namespace PlantSafety.Infrastructure.Analytics.PlatformHealthDashboard.Capture;

/// <summary>
/// Periodically flushes queued <see cref="RequestMetricSample"/> instances into
/// <c>[PlatformHealth].[RequestMetricEvent]</c>.
/// </summary>
public sealed class RequestMetricsCaptureService(
    ChannelRequestMetricsRecorder recorder,
    ISqlConnectionFactory connectionFactory,
    ILogger<RequestMetricsCaptureService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(InfrastructureConstants.RequestMetricsFlushIntervalSeconds);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RequestMetricsCaptureService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await FlushBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Metrics capture is best-effort observability, never a hard dependency -- log
                // and continue rather than crashing the hosted service (Handbook §2.19-2.22:
                // single structured error envelope, no message template).
                logger.LogError(ex, "RequestMetricsCaptureService encountered an error during flush.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown.
            }
        }

        logger.LogInformation("RequestMetricsCaptureService stopped.");
    }

    private async Task FlushBatchAsync(CancellationToken cancellationToken)
    {
        var batch = new List<RequestMetricSample>(InfrastructureConstants.RequestMetricsMaxBatchSize);

        while (batch.Count < InfrastructureConstants.RequestMetricsMaxBatchSize
               && recorder.Reader.TryRead(out var sample))
        {
            batch.Add(sample);
        }

        if (batch.Count == 0)
        {
            return;
        }

        using var conn = connectionFactory.CreateConnection();

        var parameters = batch.Select(s => new
        {
            s.TenantId,
            s.CorrelationId,
            s.Method,
            s.Path,
            s.StatusCode,
            s.ElapsedMs,
            s.OccurredAtUtc
        });

        // Dapper natively batches an IEnumerable<TParam> into one multi-exec round trip.
        await conn.ExecuteAsync(
            "[PlatformHealth].[usp_RequestMetricEvent_InsertBatch]",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}
