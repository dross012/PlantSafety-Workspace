// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           TenantTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 11 (Multi-Tenant & Contextual Access Modernization)
//                  deliverable. Reports a TenantContextResolution and a
//                  TenantAccessDecision through the existing Phase 8
//                  Shared.Telemetry primitives (ILogEventWriter/LogEvent,
//                  PlantSafetyMetrics) -- the same pattern AuthenticationTelemetry
//                  (Phase 10) and AuthorizationTelemetry (Phase 9) already
//                  established. No new logging or metrics primitive.
// ============================================================================
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Reports tenant resolution and isolation-check outcomes through Shared.Telemetry's existing primitives.</summary>
public static class TenantTelemetry
{
    /// <summary>Records <paramref name="resolution"/>: increments TenantResolutionsPerformed and writes one TENANT_RESOLVED log event.</summary>
    public static void RecordResolution(TenantContextResolution resolution, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        string outcome = !resolution.TenantKeyProvided ? "NoKeyProvided" : resolution.TenantRecognized ? "Recognized" : "Unrecognized";

        PlantSafetyMetrics.TenantResolutionsPerformed.Add(1, new KeyValuePair<string, object?>("outcome", outcome));

        logEventWriter.Write(new LogEvent(
            EventId: "TENANT_RESOLVED",
            Category: LogCategory.Tenant,
            Severity: resolution.TenantKeyProvided && !resolution.TenantRecognized ? LogSeverity.Warning : LogSeverity.Info,
            Message: $"Tenant resolution: {outcome}. Resolved tenant \"{resolution.Context.TenantId}\".",
            Payload: new Dictionary<string, object?>
            {
                ["Outcome"] = outcome,
                ["TenantId"] = resolution.Context.TenantId,
                ["SchemaVersion"] = TenantContext.SchemaVersion,
            },
            CorrelationId: resolution.Context.CorrelationId,
            Module: "Tenant",
            OccurredAtUtc: clock.UtcNow));
    }

    /// <summary>
    /// Records <paramref name="decision"/> for a tenant-isolation check against <paramref name="module"/>:
    /// increments TenantIsolationChecksPerformed (and TenantIsolationDenials on deny), and writes a
    /// TENANT_ISOLATION_CHECK_PERFORMED log event.
    /// </summary>
    public static void RecordIsolationDecision(TenantAccessDecision decision, TenantContext tenantContext, string module, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(tenantContext);

        string outcome = decision.IsAuthorized ? "Allow" : "Deny";
        PlantSafetyMetrics.TenantIsolationChecksPerformed.Add(1,
            new KeyValuePair<string, object?>("module", module),
            new KeyValuePair<string, object?>("outcome", outcome));

        if (!decision.IsAuthorized)
        {
            PlantSafetyMetrics.TenantIsolationDenials.Add(1, new KeyValuePair<string, object?>("module", module));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "TENANT_ISOLATION_CHECK_PERFORMED",
            Category: LogCategory.Tenant,
            Severity: decision.IsAuthorized ? LogSeverity.Info : LogSeverity.Warning,
            Message: $"Tenant isolation check for module \"{module}\", tenant \"{tenantContext.TenantId}\": {outcome}. {decision.Reason}",
            Payload: new Dictionary<string, object?>
            {
                ["Module"] = module,
                ["TenantId"] = tenantContext.TenantId,
                ["Outcome"] = outcome,
                ["Reason"] = decision.Reason,
            },
            CorrelationId: tenantContext.CorrelationId,
            Module: "Tenant",
            OccurredAtUtc: clock.UtcNow));
    }
}
