// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing IncidentManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Factories;

/// <summary>Default factory implementation for constructing <see cref="IncidentManagement"/> aggregates.</summary>
public sealed class IncidentManagementFactory : IIncidentManagementFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
