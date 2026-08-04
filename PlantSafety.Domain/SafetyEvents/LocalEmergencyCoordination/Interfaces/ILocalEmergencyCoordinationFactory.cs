// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           ILocalEmergencyCoordinationFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing LocalEmergencyCoordination aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.LocalEmergencyCoordination"/> aggregates.</summary>
public interface ILocalEmergencyCoordinationFactory
{
    /// <summary>Creates a new LocalEmergencyCoordination aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid? incidentId = null,
        string caseName = "");
}
