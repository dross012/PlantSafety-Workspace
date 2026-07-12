// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.CompetencyManagement
//  Layer:          Domain
//  File:           CompetencyManagementCreatedEvent.cs
//  Created:        2026-07-12
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.CompetencyManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.CompetencyManagement.Events;

/// <summary>Raised when a new <see cref="CompetencyManagement"/> aggregate is created.</summary>
public sealed record CompetencyManagementCreatedEvent(
    CompetencyManagementEventId EventId,
    CompetencyManagementId      CompetencyManagementId,
    UtcTimestamp      CreatedAtUtc,
    PerformedByActor  Actor,
    CorrelationId     CorrelationId,
    CausationId       CausationId
) : ICompetencyManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CompetencyManagementCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
