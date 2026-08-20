// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureSetEmptyException.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a caller attempts to publish a FeatureSet that contains no
//                  features. Ported concept from the orphaned, dead
//                  Intelligence.Domain/FeatureEngineering/Entities/FeatureSet.cs's inline
//                  InvalidOperationException throw in Publish().
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.FeatureEngineering.Exceptions;

/// <summary>Thrown when a caller attempts to publish a FeatureSet with zero features.</summary>
public sealed class FeatureSetEmptyException : FeatureEngineeringDomainException
{
    /// <summary>Creates the exception for the given feature set identity.</summary>
    public FeatureSetEmptyException(FeatureSetId id, string name, string version, CorrelationId correlationId, CausationId causationId)
        : base(
            $"FeatureSet '{id}' ('{name}' v{version}) must contain at least one feature before it can be published.",
            "FeatureEngineering.EmptyFeatureSet",
            "Validation",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
