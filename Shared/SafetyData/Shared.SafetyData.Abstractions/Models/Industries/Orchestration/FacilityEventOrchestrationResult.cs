// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Orchestration
//  Layer:          Abstractions
//  File:           FacilityEventOrchestrationResult.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Result of interpreting a single real runtime event (from
//                  PlantSafety's AccidentManagement/RiskManagement/etc. domains,
//                  delivered via Shared.EventBus) against a resolved facility.
//                  Deliberately NOT tied to one of Eventing.EventCatalog's 21
//                  specific hazard-typed event codes: the real domain events
//                  available today (AccidentCreated, RiskCreated, etc.) are
//                  generic aggregate-lifecycle events with no hazard-type
//                  field, so there is no real data to determine which
//                  EventCatalog entry (chemical spill vs. fire vs. injury,
//                  etc.) applies -- see EventOrchestrationService.cs's header
//                  for the full investigation trail. What IS grounded in real
//                  data: a real event occurred for a real (resolved) tenant,
//                  so this result recomputes that facility's actual
//                  compliance/risk posture, which is exactly what "Event ->
//                  Compliance updates" in the brief asks for.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

namespace Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

/// <summary>
/// The facility-level impact of a single real runtime event: the resolved facility's
/// recomputed compliance/risk posture at the moment the event occurred.
/// </summary>
/// <param name="TenantId">The tenant the triggering event was published for.</param>
/// <param name="FacilityId">The facility resolved for <paramref name="TenantId"/>.</param>
/// <param name="SourceModule">The publishing module (<c>EventEnvelope.SourceModule</c>, e.g. "PlantSafety").</param>
/// <param name="EventType">The canonical event type (<c>EventEnvelope.EventType</c>, e.g. "Accidents.AccidentCreated").</param>
/// <param name="OccurredAtUtc">UTC instant the underlying domain fact occurred.</param>
/// <param name="ComplianceScore">The facility's recomputed compliance score.</param>
/// <param name="RiskScore">The facility's recomputed multi-dimensional risk score.</param>
/// <param name="ComplianceReadiness">The facility's recomputed compliance readiness.</param>
/// <param name="RiskReadiness">The facility's recomputed risk readiness.</param>
/// <param name="DashboardAlerts">The union of the facility's compliance- and risk-dashboard alerts at this moment.</param>
/// <param name="Notification">A deterministic, human-readable summary suitable for a notification message.</param>
public sealed record FacilityEventOrchestrationResult(
    string TenantId,
    Guid FacilityId,
    string SourceModule,
    string EventType,
    DateTime OccurredAtUtc,
    ComplianceScore ComplianceScore,
    RiskScore RiskScore,
    ComplianceReadiness ComplianceReadiness,
    RiskReadiness RiskReadiness,
    IReadOnlyList<string> DashboardAlerts,
    string Notification);
