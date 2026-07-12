// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           IRiskManagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing RiskManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Interfaces;

/// <summary>Factory interface for constructing <see cref="RiskManagement"/> aggregates.</summary>
public interface IRiskManagementFactory
{
    /// <summary>Creates a new RiskManagement aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
