// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureSetPublishRules.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Guards for the FeatureSet entity's mutation/publish lifecycle. Ported concept
//                  from the orphaned, dead
//                  Intelligence.Domain/FeatureEngineering/Entities/FeatureSet.cs's inline
//                  AddFeature/Publish guard checks, extracted into this module's own static-Rules
//                  convention. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Rules.DataLakeActivityLifecycleRules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.FeatureEngineering.Rules;

/// <summary>Defines permitted mutations for the FeatureSet entity.</summary>
public static class FeatureSetPublishRules
{
    /// <summary>Returns <see langword="true"/> when a feature may still be added (not yet published).</summary>
    public static bool CanAddFeature(bool isPublished) => !isPublished;

    /// <summary>Returns <see langword="true"/> when the feature set is ready to publish: not yet published and contains at least one feature.</summary>
    public static bool CanPublish(bool isPublished, int featureCount) => !isPublished && featureCount > 0;
}
