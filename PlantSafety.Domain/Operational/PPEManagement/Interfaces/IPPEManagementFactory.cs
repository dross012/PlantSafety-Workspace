// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           IPPEManagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PPEManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Interfaces;

/// <summary>Factory interface for constructing <see cref="PPEManagement"/> aggregates.</summary>
public interface IPPEManagementFactory
{
    /// <summary>Creates a new PPEManagement aggregate with the supplied parameters.</summary>
    PPEManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
