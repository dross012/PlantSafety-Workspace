// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Events
//  File:           UnifiedEventGovernance.cs
//  Created:        2026-08-10
//  Description:    Phase 12 (Unified Contract & Schema Modernization)
//                  deliverable. Shared.Telemetry.Abstractions.Events.LogEvent
//                  (Phase 2.4) is already the single event shape every
//                  module -- WorkflowEngine, Facility, Audit, Authorization,
//                  Authentication, Tenant, Governance -- emits through today
//                  (confirmed by direct inspection of every *Telemetry.cs
//                  file across Phases 8-11 before writing this file: all of
//                  them construct the same `new LogEvent(EventId, Category,
//                  Severity, Message, Payload, CorrelationId, Module,
//                  OccurredAtUtc)`). No new "UnifiedEventBase" record was
//                  introduced -- doing so would create the exact "parallel
//                  telemetry system" this phase's own instructions forbid.
//
//                  What this file adds: a pure check that a given LogEvent's
//                  CorrelationId is consistent with the ContractMetadata the
//                  request that produced it carried -- the actual
//                  "unification" gap (nothing enforced this consistency
//                  before), not a new shape.
// ============================================================================
using Shared.Authorization.Abstractions.Contracts;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Authorization.Infrastructure.Events;

/// <summary>Pure validation that a LogEvent's correlation id is consistent with the ContractMetadata of the request that produced it.</summary>
public static class UnifiedEventGovernance
{
    /// <summary>
    /// Validates <paramref name="logEvent"/> against <paramref name="metadata"/>. Reports
    /// "MissingLogEvent" when null; "EventCorrelationIdMismatch" when the event's CorrelationId
    /// does not match the metadata's.
    /// </summary>
    public static IReadOnlyList<string> ValidateCorrelationConsistency(ILogEvent? logEvent, ContractMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var violations = new List<string>();

        if (logEvent is null)
        {
            violations.Add("MissingLogEvent");
            return violations;
        }

        if (!string.Equals(logEvent.CorrelationId, metadata.CorrelationId, StringComparison.Ordinal))
        {
            violations.Add("EventCorrelationIdMismatch");
        }

        return violations;
    }
}
