// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           IManagementOfChangeFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing ManagementOfChange aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces;

/// <summary>Factory interface for constructing <see cref="ManagementOfChange"/> aggregates.</summary>
public interface IManagementOfChangeFactory
{
    /// <summary>Creates a new ManagementOfChange aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
