// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactAssessedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new CommunityImpactAssessment aggregate is recorded for an
//                  on-site SafetyEvents incident. This is the SafetyEvents integration point: the
//                  IncidentId is carried as a weak Guid reference only (no direct type reference
//                  to the SafetyEvents module), per the Canonical Adapter Rule / Zero Cross-Module
//                  Leakage rule. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Events;

/// <summary>Raised when a new <see cref="Aggregates.CommunityImpactAssessment"/> aggregate is recorded.</summary>
public sealed record CommunityImpactAssessedEvent(
    CommunityImpactAssessmentEventId EventId,
    CommunityImpactAssessmentId      CommunityImpactAssessmentId,
    Guid                              IncidentId,
    CommunityImpactSeverity           Severity,
    UtcTimestamp                      AssessedAtUtc,
    PerformedByActor                  Actor,
    CorrelationId                     CorrelationId,
    CausationId                       CausationId
) : ICommunityImpactAssessmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunityImpactAssessed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AssessedAtUtc;
}
