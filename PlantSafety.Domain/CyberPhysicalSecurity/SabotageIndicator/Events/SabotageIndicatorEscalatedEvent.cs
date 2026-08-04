// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageIndicatorEscalatedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when correlated multi-source indicators trigger auto-escalation per the
//                  module's risk-hook requirement ("correlated multi-source indicators
//                  auto-escalate"). Typically drives OpenCyberPhysicalIncident.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Events;

/// <summary>Raised when a <see cref="Aggregates.SabotageIndicator"/> is auto-escalated due to multi-source correlation.</summary>
public sealed record SabotageIndicatorEscalatedEvent(
    SabotageIndicatorEventId EventId,
    SabotageIndicatorId       SabotageIndicatorId,
    int                       CorrelatedIndicatorCount,
    UtcTimestamp               EscalatedAtUtc,
    PerformedByActor           Actor,
    CorrelationId               CorrelationId,
    CausationId                 CausationId
) : ISabotageIndicatorDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SabotageIndicatorEscalated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EscalatedAtUtc;
}
