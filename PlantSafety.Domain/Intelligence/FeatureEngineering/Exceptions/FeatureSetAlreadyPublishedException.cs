// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureSetAlreadyPublishedException.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a caller attempts to add a feature to, or re-publish, a FeatureSet
//                  that has already been published (immutable thereafter). Ported concept from the
//                  orphaned, dead Intelligence.Domain/FeatureEngineering/Entities/FeatureSet.cs's
//                  inline InvalidOperationException throws.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.FeatureEngineering.Exceptions;

/// <summary>Thrown when a mutation is attempted against an already-published, immutable FeatureSet.</summary>
public sealed class FeatureSetAlreadyPublishedException : FeatureEngineeringDomainException
{
    /// <summary>Creates the exception for the given feature set identity.</summary>
    public FeatureSetAlreadyPublishedException(FeatureSetId id, string name, string version, CorrelationId correlationId, CausationId causationId)
        : base(
            $"FeatureSet '{id}' ('{name}' v{version}) is already published and is immutable.",
            "FeatureEngineering.AlreadyPublished",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
