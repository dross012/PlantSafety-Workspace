// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           ICultureFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Culture aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Interfaces;

/// <summary>Factory interface for constructing <see cref="Culture"/> aggregates.</summary>
public interface ICultureFactory
{
    /// <summary>Creates a new Culture aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Analytics.Culture.Aggregates.Culture Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
