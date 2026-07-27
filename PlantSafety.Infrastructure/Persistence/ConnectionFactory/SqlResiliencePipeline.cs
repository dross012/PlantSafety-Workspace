// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           SqlResiliencePipeline.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (data-access-modernization-agent)
//  SchemaVersion:  2026.1
//  Description:    Polly 8 resilience pipeline for transient SQL Server connection-open
//                  errors. Used by SqlConnectionFactory to retry SqlConnection.Open()/
//                  OpenAsync() with exponential back-off and jitter. Only retries on
//                  known transient error numbers; permanent errors (auth failures, schema
//                  errors) surface immediately.
//
//                  JUDGMENT CALL (data-access-modernization pass, Checklist Section 2,
//                  item 11.1): PlantSafety's own SqlConnectionFactory previously opened
//                  connections with no retry policy at all, while the sibling Intelligence
//                  module (Intelligence.Infrastructure.Shared.Resilience.
//                  SqlResiliencePipeline) already established a Polly-based retry pipeline
//                  for exactly this purpose. This file is a deliberate, minimal port of
//                  that established pattern (same transient error-number table, same
//                  back-off shape) rather than inventing a new convention, per the
//                  Handbook's "follow the established pattern" rule. Kept private to
//                  Infrastructure and wired in behind SqlConnectionFactory's existing
//                  public API so no repository call site needed to change.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace PlantSafety.Infrastructure.Persistence.ConnectionFactory;

/// <summary>
/// Builds a Polly 8 resilience pipeline for SQL Server connection-open retries. Mirrors
/// Intelligence.Infrastructure.Shared.Resilience.SqlResiliencePipeline's transient-error
/// classification and back-off shape.
/// </summary>
internal static class SqlResiliencePipeline
{
    // SQL Server transient error numbers (same table as Intelligence.Infrastructure's
    // SqlResiliencePipeline):
    // 1205  – Deadlock victim
    //   -2  – Timeout expired
    //  233  – Connection initialization error
    //   64  – Connection was established then an error occurred
    // 4060  – Cannot open database (often transient during failover)
    // 40613 – Database on server is not currently available
    // 40197 – Service encountered an error processing the request
    // 40501 – Service is currently busy
    // 49918 – Cannot process request; not enough resources
    private static readonly int[] TransientErrorNumbers = [1205, -2, 233, 64, 4060, 40613, 40197, 40501, 49918];

    /// <summary>
    /// Creates a retry pipeline that handles transient <see cref="SqlException"/> failures
    /// raised while opening a connection.
    /// </summary>
    /// <param name="logger">Logger for retry telemetry. Never logs connection strings or credentials.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (0 = no retries).</param>
    /// <param name="operationName">Label for the operation being retried, used only in log output.</param>
    internal static ResiliencePipeline<SqlConnection> CreateConnectionPipeline(
        ILogger logger,
        int maxRetries,
        string operationName)
    {
        return new ResiliencePipelineBuilder<SqlConnection>()
            .AddRetry(new RetryStrategyOptions<SqlConnection>
            {
                MaxRetryAttempts = maxRetries,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<SqlConnection>()
                    .Handle<SqlException>(IsTransient),
                OnRetry = args =>
                {
                    logger.LogWarning("{@SqlRetry}", new
                    {
                        OperationName = operationName,
                        Attempt = args.AttemptNumber + 1,
                        MaxRetries = maxRetries,
                        ErrorNumber = (args.Outcome.Exception as SqlException)?.Number,
                        ExceptionType = args.Outcome.Exception?.GetType().Name
                    });
                    return default;
                }
            })
            .Build();
    }

    private static bool IsTransient(SqlException ex) =>
        TransientErrorNumbers.Contains(ex.Number);
}
