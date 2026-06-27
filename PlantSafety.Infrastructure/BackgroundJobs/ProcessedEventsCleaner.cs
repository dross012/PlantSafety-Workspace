// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.BackgroundJobs
//  Layer:          Infrastructure
//  File:           ProcessedEventsCleaner.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Hosted service that periodically purges old processed-events idempotency records.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Hosting;

namespace PlantSafety.Infrastructure.BackgroundJobs;

/// <summary>
/// Hosted service that periodically purges stale records from <c>System.ProcessedEvents</c>
/// to prevent unbounded table growth while preserving the idempotency window.
/// </summary>
public sealed class ProcessedEventsCleaner(
    ISqlConnectionFactory connectionFactory,
    ILogger<ProcessedEventsCleaner> logger)
    : BackgroundService
{
    private static readonly TimeSpan RetentionWindow = TimeSpan.FromDays(30);
    private static readonly TimeSpan RunInterval     = TimeSpan.FromHours(6);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ProcessedEventsCleaner started. Retention window: {Days} days.", RetentionWindow.TotalDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var conn = connectionFactory.CreateConnection();
                var cutoff  = DateTime.UtcNow - RetentionWindow;
                var deleted = await conn.ExecuteAsync(
                    sql: $"DELETE FROM {SqlConstants.ProcessedEventsTable} WHERE ProcessedAt < @Cutoff",
                    param: new { Cutoff = cutoff },
                    commandType: CommandType.Text);

                if (deleted > 0)
                    logger.LogInformation("ProcessedEventsCleaner purged {Count} stale records.", deleted);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "ProcessedEventsCleaner encountered an error.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }
}
