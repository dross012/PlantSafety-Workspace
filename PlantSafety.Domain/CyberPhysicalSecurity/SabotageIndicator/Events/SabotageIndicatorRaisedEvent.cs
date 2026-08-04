// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageIndicatorRaisedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new sabotage indicator is raised. Feeds RiskManagement at
//                  elevated priority per the module's risk-hook requirement.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Events;

/// <summary>Raised when a new <see cref="Aggregates.SabotageIndicator"/> is raised.</summary>
public sealed record SabotageIndicatorRaisedEvent(
    SabotageIndicatorEventId EventId,
    SabotageIndicatorId       SabotageIndicatorId,
    SabotageIndicatorSource   Source,
    SabotageIndicatorSeverity Severity,
    UtcTimestamp               RaisedAtUtc,
    PerformedByActor           Actor,
    CorrelationId               CorrelationId,
    CausationId                 CausationId
) : ISabotageIndicatorDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SabotageIndicatorRaised";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RaisedAtUtc;
}
