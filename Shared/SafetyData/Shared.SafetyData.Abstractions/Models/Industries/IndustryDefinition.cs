// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           IndustryDefinition.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable schema for a single entry in IndustryRegistry.
//                  Fields map 1:1 to the source industry taxonomy: TopLevel
//                  category, SubIndustries (may be empty when the source
//                  taxonomy did not enumerate any), Regulators, and Flags.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// Deterministic, immutable definition of a single industry within the
/// PlantSafety industry taxonomy.
/// </summary>
/// <param name="Name">The industry's canonical name (e.g. "Chemical Manufacturing").</param>
/// <param name="TopLevelCategory">The top-level taxonomy category this industry belongs to.</param>
/// <param name="SubIndustries">Named sub-industries within this industry; empty if the source taxonomy did not enumerate any.</param>
/// <param name="Regulators">The regulators that apply to this industry, in source order.</param>
/// <param name="Flags">The combined safety/compliance flags that apply to this industry.</param>
public sealed record IndustryDefinition(
    string Name,
    string TopLevelCategory,
    IReadOnlyList<string> SubIndustries,
    IReadOnlyList<Regulator> Regulators,
    SafetyFlag Flags);
