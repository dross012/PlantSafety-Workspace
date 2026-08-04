// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing LocalEmergencyCoordination aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.LocalEmergencyCoordination"/> aggregates.</summary>
public sealed class LocalEmergencyCoordinationFactory : Interfaces.ILocalEmergencyCoordinationFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid? incidentId = null,
        string caseName = "")
        => PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, incidentId, caseName);
}
