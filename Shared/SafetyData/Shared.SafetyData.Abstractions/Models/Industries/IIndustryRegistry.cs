// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           IIndustryRegistry.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-friendly abstraction over the static IndustryRegistry, so
//                  consumers can take a constructor dependency instead of a
//                  static reference. Surface mirrors IndustryRegistry's public
//                  members exactly (All, FindByName, ByTopLevelCategory).
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// DI-resolvable read-only view over the PlantSafety industry taxonomy defined
/// in <see cref="IndustryRegistry"/>.
/// </summary>
public interface IIndustryRegistry
{
    /// <summary>Every industry in the PlantSafety industry taxonomy, in taxonomy order.</summary>
    IReadOnlyList<IndustryDefinition> All { get; }

    /// <summary>Looks up an industry definition by its exact, canonical name.</summary>
    IndustryDefinition? FindByName(string name);

    /// <summary>All industry definitions belonging to the given top-level category.</summary>
    IReadOnlyList<IndustryDefinition> ByTopLevelCategory(string topLevelCategory);
}
