// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsInvalidComputationException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Thrown when GlobalSafetyInsightsAggregate.Compute is invoked with input that
//                  violates a module invariant (Rules.GlobalSafetyInsightsInvariantRules).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Exceptions;

/// <summary>Thrown when snapshot-computation input violates a GlobalSafetyInsights invariant.</summary>
public sealed class GlobalSafetyInsightsInvalidComputationException : GlobalSafetyInsightsDomainException
{
    /// <summary>Creates a new instance.</summary>
    public GlobalSafetyInsightsInvalidComputationException(string reason, Guid correlationId, Guid causationId)
        : base($"Invalid GlobalSafetyInsights computation: {reason}", correlationId, causationId)
    {
    }
}
