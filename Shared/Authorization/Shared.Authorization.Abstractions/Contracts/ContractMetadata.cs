// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Contracts
//  File:           ContractMetadata.cs
//  Created:        2026-08-10
//  Description:    Phase 12 (Unified Contract & Schema Modernization)
//                  deliverable. The unified schema-metadata envelope every
//                  module's *Tenant.cs composes once a TenantContext is
//                  resolved (RoutingTenant.cs, ApiKeyAuthenticationMiddleware.cs).
//                  Phase 12's own precedent (documented in its commit
//                  record) is that no ContractBase/UnifiedEventBase was
//                  invented -- the existing LogEvent shape already serves as
//                  the unified event schema. ContractMetadata is the one
//                  genuinely new unified-schema type Phase 12 introduced: a
//                  small, deterministic, versioned envelope (schema version,
//                  tenant, correlation, issued-at) carried alongside a
//                  request/response, not a replacement for any existing
//                  domain type.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Authorization.Abstractions.Contracts;

/// <summary>The unified schema-metadata envelope: deterministic versioning, tenant, and correlation propagation.</summary>
public sealed record ContractMetadata(int SchemaVersion, string TenantId, string CorrelationId, DateTime IssuedAtUtc)
{
    /// <summary>The current ContractMetadata schema version. Increment only on a breaking field-shape change.</summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>Writes one CONTRACT_METADATA_EMITTED log event recording this metadata's composition.</summary>
    public void RecordEmitted(IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        logEventWriter.Write(new LogEvent(
            EventId: "CONTRACT_METADATA_EMITTED",
            Category: LogCategory.Authorization,
            Severity: LogSeverity.Info,
            Message: $"ContractMetadata composed for tenant \"{TenantId}\" (schema v{SchemaVersion}).",
            Payload: new Dictionary<string, object?>
            {
                ["SchemaVersion"] = SchemaVersion,
                ["TenantId"] = TenantId,
                ["IssuedAtUtc"] = IssuedAtUtc,
            },
            CorrelationId: CorrelationId,
            Module: "Authorization",
            OccurredAtUtc: clock.UtcNow));
    }
}
