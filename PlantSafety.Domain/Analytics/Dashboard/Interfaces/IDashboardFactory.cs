// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           IDashboardFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Dashboard aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Interfaces;

/// <summary>Factory interface for constructing <see cref="Dashboard"/> aggregates.</summary>
public interface IDashboardFactory
{
    /// <summary>Creates a new Dashboard aggregate with the supplied parameters.</summary>
    Dashboard Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
