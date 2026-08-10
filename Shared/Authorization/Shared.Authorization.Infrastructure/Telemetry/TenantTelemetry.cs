// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           TenantTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Records TenantContextResolution
//                  outcomes (via ApiKeyAuthenticationMiddleware) and
//                  TenantAccessDecision outcomes (via every module's
//                  *Tenant.cs) as metric increments and structured log
//                  events.
// ============================================================================
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Records tenant resolution and tenant-isolation-check telemetry.</summary>
public static class TenantTelemetry
{
    /// <summary>Records <paramref name="resolution"/>'s outcome as a metric increment and a structured log event.</summary>
    public static void RecordResolution(TenantContextResolution resolution, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        PlantSafetyMetrics.TenantResolutionsPerformed.Add(1, new KeyValuePair<string, object?>("outcome", resolution.Outcome.ToString()));

        logEventWriter.Write(new LogEvent(
            EventId: "TENANT_RESOLUTION_PERFORMED",
            Category: LogCategory.Tenant,
            Severity: LogSeverity.Info,
            Message: $"Tenant resolution outcome: {resolution.Outcome} (resolved tenant \"{resolution.Context.TenantId}\").",
            Payload: new Dictionary<string, object?>
            {
                ["Outcome"] = resolution.Outcome.ToString(),
                ["TenantId"] = resolution.Context.TenantId,
            },
            CorrelationId: resolution.Context.CorrelationId,
            Module: "Tenant",
            OccurredAtUtc: clock.UtcNow));
    }

    /// <summary>Records <paramref name="decision"/> as a metric increment (and denial increment, if denied) and a structured log event.</summary>
    public static void RecordIsolationDecision(TenantAccessDecision decision, TenantContext tenantContext, string module, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(tenantContext);
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        PlantSafetyMetrics.TenantIsolationChecksPerformed.Add(1, new KeyValuePair<string, object?>("module", module), new KeyValuePair<string, object?>("outcome", decision.IsAuthorized ? "Allow" : "Deny"));

        if (!decision.IsAuthorized)
        {
            PlantSafetyMetrics.TenantIsolationDenials.Add(1, new KeyValuePair<string, object?>("module", module));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "TENANT_ISOLATION_CHECK_PERFORMED",
            Category: LogCategory.Tenant,
            Severity: decision.IsAuthorized ? LogSeverity.Info : LogSeverity.Warning,
            Message: $"Tenant isolation check for module \"{module}\", tenant \"{tenantContext.TenantId}\": {(decision.IsAuthorized ? "allowed" : "denied")} -- {decision.Reason}",
            Payload: new Dictionary<string, object?>
            {
                ["Module"] = module,
                ["TenantId"] = tenantContext.TenantId,
                ["IsAuthorized"] = decision.IsAuthorized,
                ["Reason"] = decision.Reason,
            },
            CorrelationId: tenantContext.CorrelationId,
            Module: module,
            OccurredAtUtc: clock.UtcNow));
    }
}
