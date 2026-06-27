// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IIncidentManagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing IncidentManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Interfaces;

/// <summary>Factory interface for constructing <see cref="IncidentManagement"/> aggregates.</summary>
public interface IIncidentManagementFactory
{
    /// <summary>Creates a new IncidentManagement aggregate with the supplied parameters.</summary>
    IncidentManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
