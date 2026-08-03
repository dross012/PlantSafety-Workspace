// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           CrossSiteActionEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Phase 8 §4 internal cross-site event-mesh contract for a coordinated action
//                  spanning multiple sites (e.g. "roll out corrective action X to sites A, B, C").
//                  Consumed by CrossSiteActionConsumer, which records the action into this module's
//                  audit trail. NOT YET RAISED anywhere -- the commands that publish this event
//                  belong to the Multi-Site Coordination workflow (Phase 8 §1/§3), built by a later
//                  agent. Defined here, now, as a stable contract so that agent's command handlers
//                  have a real, working consumer to publish into from day one. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

namespace PlantSafety.Domain.GlobalSafetyInsights.Events;

/// <summary>A coordinated action initiated across two or more sites.</summary>
public sealed record CrossSiteActionEvent(
    GlobalSafetyInsightsEventId EventId,
    Guid                        TenantId,
    string                      RegionCode,
    Guid                        SiteId,
    IReadOnlyList<Guid>         AffectedSiteIds,
    string                      ActionType,
    string                      Description,
    Guid?                       InitiatedByUserId,
    UtcTimestamp                InitiatedAtUtc,
    PerformedByActor            Actor,
    CorrelationId               CorrelationId,
    CausationId                 CausationId,
    Guid?                       WorkflowInstanceId = null
) : IGlobalSafetyInsightsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CrossSiteAction";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => InitiatedAtUtc;
}
