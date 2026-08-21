// ============================================================================
//  Project:        Shared.AuditLogging.Infrastructure
//  Module:         Telemetry
//  File:           AuditTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 8 (Observability, Telemetry, Diagnostics, and
//                  Governance) deliverable. TelemetryAuditLogRepository is a
//                  decorator over the real IAuditLogRepository
//                  (AuditLogDapperRepository, unchanged by this phase --
//                  still read-only, still SearchAsync only), adding
//                  deterministic logging, metrics (search count + duration
//                  histogram), and reliability reporting around the search
//                  call. Adds no new method to IAuditLogRepository and no
//                  new business logic -- Audit remains read-only.
//
//                  HONEST CAVEAT: IAuditLogRepository has no live caller
//                  anywhere in PlantSafety.UI.Api today -- confirmed by
//                  direct search before writing this file (no AuditController
//                  exists, AddSharedAuditLogging is never called from
//                  Program.cs). This decorator is registered as a drop-in
//                  replacement for whenever Audit search is wired into a
//                  real host, per this phase's "instrument what exists,
//                  don't invent a caller" scope -- it is not, itself, a new
//                  caller.
// ============================================================================
using Shared.AuditLogging.Abstractions.Models;
using Shared.AuditLogging.Abstractions.Repositories;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;
using Shared.Telemetry.Abstractions.Reliability;

namespace Shared.AuditLogging.Infrastructure.Telemetry;

/// <summary>Decorates an IAuditLogRepository with deterministic logging, metrics, and reliability reporting around SearchAsync.</summary>
public sealed class TelemetryAuditLogRepository(
    IAuditLogRepository inner,
    IClock clock,
    ILogEventWriter logEventWriter,
    ICorrelationContext correlationContext) : IAuditLogRepository
{
    private const string Module = "Audit";

    /// <inheritdoc />
    public async Task<AuditLogSearchResult> SearchAsync(AuditLogSearchCriteria criteria, CancellationToken cancellationToken)
    {
        DateTime startedAtUtc = clock.UtcNow;
        AuditLogSearchResult result;
        try
        {
            result = await ReliabilityHooks.ExecuteAsync(
                Module,
                nameof(IAuditLogRepository.SearchAsync),
                clock,
                logEventWriter,
                correlationContext.CorrelationId,
                () => inner.SearchAsync(criteria, cancellationToken));
        }
        catch
        {
            PlantSafetyMetrics.AuditSearchesExecuted.Add(1, new KeyValuePair<string, object?>("outcome", "Failure"));
            throw;
        }

        double durationMs = (clock.UtcNow - startedAtUtc).TotalMilliseconds;
        PlantSafetyMetrics.AuditSearchesExecuted.Add(1, new KeyValuePair<string, object?>("outcome", "Success"));
        PlantSafetyMetrics.AuditSearchDurationMs.Record(durationMs);

        logEventWriter.Write(new LogEvent(
            EventId: "AUDIT_SEARCH_EXECUTED",
            Category: LogCategory.Audit,
            Severity: LogSeverity.Info,
            Message: $"Audit search returned {result.Items.Count} of {result.TotalCount} matching row(s) (page {result.PageNumber}, size {result.PageSize}) in {durationMs:F1}ms.",
            Payload: new Dictionary<string, object?>
            {
                ["ItemCount"] = result.Items.Count,
                ["TotalCount"] = result.TotalCount,
                ["PageNumber"] = result.PageNumber,
                ["PageSize"] = result.PageSize,
                ["DurationMs"] = durationMs,
            },
            CorrelationId: correlationContext.CorrelationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));

        return result;
    }
}
