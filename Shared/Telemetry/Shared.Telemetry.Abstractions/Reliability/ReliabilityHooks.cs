// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Reliability
//  File:           ReliabilityHooks.cs
//  Created:        2026-08-10
//  Description:    Phase 8 (Observability, Telemetry, Diagnostics, and
//                  Governance) deliverable. A single, generic reliability
//                  decorator -- not a per-module reimplementation -- used by
//                  every module's telemetry wrapper (WorkflowEngineTelemetry,
//                  FacilityTelemetry, AuditTelemetry) to capture failures,
//                  durations, and outcomes deterministically around an
//                  existing pure/read-only operation. Never alters the
//                  wrapped operation's behavior: on success it returns the
//                  operation's own result unchanged; on failure it logs and
//                  rethrows the original exception unchanged. Built entirely
//                  on Shared.Telemetry's existing ILogEventWriter/LogEvent
//                  and Shared.Deterministic's IClock -- no new logging,
//                  metrics, or tracing primitive is introduced.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Telemetry.Abstractions.Reliability;

/// <summary>Wraps an operation with deterministic duration/outcome logging and metrics, without altering its behavior.</summary>
public static class ReliabilityHooks
{
    /// <summary>
    /// Executes <paramref name="operation"/>, recording its duration (via <paramref name="clock"/>, never
    /// the wall clock) and outcome through <paramref name="logEventWriter"/> and
    /// <see cref="PlantSafetyMetrics.ReliabilityOperationDurationMs"/>. On failure, also increments
    /// <see cref="PlantSafetyMetrics.ReliabilityFailures"/> and rethrows the original exception unchanged --
    /// this hook never swallows a failure or substitutes a fallback result.
    /// </summary>
    public static async Task<TResult> ExecuteAsync<TResult>(
        string module,
        string operationName,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId,
        Func<Task<TResult>> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        DateTime startedAtUtc = clock.UtcNow;
        try
        {
            TResult result = await operation();
            RecordOutcome(module, operationName, clock, logEventWriter, correlationId, startedAtUtc, succeeded: true, exception: null);
            return result;
        }
        catch (Exception ex)
        {
            RecordOutcome(module, operationName, clock, logEventWriter, correlationId, startedAtUtc, succeeded: false, exception: ex);
            throw;
        }
    }

    private static void RecordOutcome(
        string module,
        string operationName,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId,
        DateTime startedAtUtc,
        bool succeeded,
        Exception? exception)
    {
        DateTime endedAtUtc = clock.UtcNow;
        double durationMs = (endedAtUtc - startedAtUtc).TotalMilliseconds;

        var tags = new KeyValuePair<string, object?>[]
        {
            new("module", module),
            new("operation", operationName),
        };
        PlantSafetyMetrics.ReliabilityOperationDurationMs.Record(durationMs, tags);
        if (!succeeded)
        {
            PlantSafetyMetrics.ReliabilityFailures.Add(1, tags);
        }

        logEventWriter.Write(new LogEvent(
            EventId: succeeded ? "RELIABILITY_OPERATION_SUCCEEDED" : "RELIABILITY_OPERATION_FAILED",
            Category: LogCategoryClassifier.Classify(module),
            Severity: succeeded ? LogSeverity.Info : LogSeverity.Error,
            Message: succeeded
                ? $"{module}.{operationName} completed in {durationMs:F1}ms."
                : $"{module}.{operationName} failed after {durationMs:F1}ms: {exception!.Message}",
            Payload: new Dictionary<string, object?>
            {
                ["DurationMs"] = durationMs,
                ["ExceptionType"] = exception?.GetType().Name,
            },
            CorrelationId: correlationId,
            Module: module,
            OccurredAtUtc: endedAtUtc));
    }
}
