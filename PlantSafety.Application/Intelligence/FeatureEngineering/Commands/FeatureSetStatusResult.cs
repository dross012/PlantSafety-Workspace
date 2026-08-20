// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           FeatureSetStatusResult.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the FeatureSet commands (Create, AddFeature,
//                  Publish). Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Commands.DataLakeActivityStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.FeatureEngineering.Commands;

/// <summary>Result payload shared by FeatureSet commands.</summary>
/// <param name="FeatureSetId">Identity of the affected FeatureSet entity.</param>
/// <param name="IsPublished">Whether the feature set is published as of this operation.</param>
/// <param name="FeatureCount">Number of features currently on the feature set.</param>
public sealed record FeatureSetStatusResult(Guid FeatureSetId, bool IsPublished, int FeatureCount);
