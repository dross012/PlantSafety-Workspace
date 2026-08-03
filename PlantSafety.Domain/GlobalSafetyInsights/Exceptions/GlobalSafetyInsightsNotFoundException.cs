// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsNotFoundException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a requested GlobalSafetyInsightsAggregate cannot be found for the
//                  given tenant. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

namespace PlantSafety.Domain.GlobalSafetyInsights.Exceptions;

/// <summary>Thrown when a requested GlobalSafetyInsightsAggregate cannot be found for the given tenant.</summary>
public sealed class GlobalSafetyInsightsNotFoundException : GlobalSafetyInsightsDomainException
{
    /// <summary>Identity of the snapshot that could not be found.</summary>
    public GlobalSafetyInsightsId GlobalSafetyInsightsId { get; }

    /// <summary>Creates a new instance.</summary>
    public GlobalSafetyInsightsNotFoundException(GlobalSafetyInsightsId globalSafetyInsightsId, Guid correlationId, Guid causationId)
        : base($"GlobalSafetyInsights snapshot '{globalSafetyInsightsId}' was not found.", correlationId, causationId)
    {
        GlobalSafetyInsightsId = globalSafetyInsightsId;
    }
}
