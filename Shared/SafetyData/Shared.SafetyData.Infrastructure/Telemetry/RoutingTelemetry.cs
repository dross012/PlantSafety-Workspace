// ============================================================================
//  Project:        Shared.SafetyData.Infrastructure
//  Module:         Telemetry
//  File:           RoutingTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 13 (Routing Modernization) deliverable.
//
//                  Scope correction (see PHASE13-README.md for the full
//                  record): the original Phase 13 prompt asked for
//                  "Diagnostics Modernization" -- Diagnostics does not exist
//                  anywhere in this program (confirmed: zero controllers,
//                  zero domain model, zero consumers; the standalone product
//                  at a different repo root is unrelated; docs/ingestion/
//                  module-flow/diagnostics.md already documented its total
//                  absence of ingress). Per the user's own correction, this
//                  phase instead modernizes Routing -- which IS real:
//                  Shared.SafetyData.Abstractions.Models.Industries.Eventing.
//                  EventRoutingEngine.Route (Phase C.8/3, already merged) is
//                  a complete, deterministic, additive-accumulation pure
//                  function -- exactly the "deterministic rule engine" this
//                  phase's instructions ask for -- that has never been
//                  wrapped with telemetry/authorization/tenant/schema
//                  governance, unlike WorkflowEngine/Facility/Audit (Phases
//                  8/9). No new rule engine, inference engine, orchestrator,
//                  scheduler, or dispatcher was invented -- EventRoutingEngine
//                  already IS the deterministic evaluation; this phase wraps
//                  it, mirroring WorkflowEngineTelemetry (Phase 8) exactly.
//
//                  Like FacilityTelemetry/AuditTelemetry before it, this
//                  wrapper has no live caller in PlantSafety.UI.Api today
//                  (IEventRoutingService/EventingServiceCollectionExtensions.
//                  AddEventing() exist but are not registered in Program.cs)
//                  -- real, wired, tested capability, documented honestly as
//                  dormant rather than pretended live.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.SafetyData.Infrastructure.Telemetry;

/// <summary>Wraps EventRoutingEngine.Route with structured logging and metrics, unchanged decision.</summary>
public static class RoutingTelemetry
{
    /// <summary>Routes <paramref name="eventDefinition"/> against <paramref name="facility"/> via EventRoutingEngine.Route, emitting telemetry.</summary>
    public static Task<EventRoutingResult> RouteWithTelemetryAsync(
        EventDefinition eventDefinition,
        FacilityProfile facility,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(eventDefinition);
        ArgumentNullException.ThrowIfNull(facility);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        EventRoutingResult result = EventRoutingEngine.Route(eventDefinition, facility);

        PlantSafetyMetrics.RoutingDecisionsComputed.Add(1, new KeyValuePair<string, object?>("eventCode", result.EventCode));

        logEventWriter.Write(new LogEvent(
            EventId: "ROUTING_DECISION_COMPUTED",
            Category: LogCategory.Routing,
            Severity: LogSeverity.Info,
            Message: $"Routed event \"{result.EventCode}\" for facility {result.FacilityId}: {result.ActivatedPrograms.Count} program(s), {result.ActivatedSops.Count} SOP(s), {result.TriggeredAudits.Count} audit(s) activated.",
            Payload: new Dictionary<string, object?>
            {
                ["ActivatedProgramCount"] = result.ActivatedPrograms.Count,
                ["ActivatedSopCount"] = result.ActivatedSops.Count,
                ["EventCode"] = result.EventCode,
                ["FacilityId"] = result.FacilityId,
                ["TriggeredAuditCount"] = result.TriggeredAudits.Count,
            },
            CorrelationId: correlationId,
            Module: "Routing",
            OccurredAtUtc: clock.UtcNow));

        return Task.FromResult(result);
    }
}
