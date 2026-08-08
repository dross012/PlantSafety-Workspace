// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Industries
//  Layer:          Infrastructure
//  File:           RegulatoryRequirementRegistryProvider.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-resolvable IRegulatoryRequirementRegistry implementation
//                  that delegates every member to the static
//                  RegulatorRequirementCatalog, RegulatorCategoryCatalog, and
//                  RegulatorRequirementMapping (Abstractions.Models.
//                  Industries). Holds no state of its own -- mirrors
//                  IndustryRegistryProvider exactly. Stateless, so a single
//                  instance is safe to share as a singleton.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Infrastructure.Industries;

/// <summary>
/// Stateless <see cref="IRegulatoryRequirementRegistry"/> implementation backed by the static
/// <see cref="RegulatorRequirementCatalog"/>, <see cref="RegulatorCategoryCatalog"/>, and
/// <see cref="RegulatorRequirementMapping"/>.
/// </summary>
public sealed class RegulatoryRequirementRegistryProvider : IRegulatoryRequirementRegistry
{
    /// <inheritdoc />
    public IReadOnlyList<RegulatorRequirement> All => RegulatorRequirementCatalog.All;

    /// <inheritdoc />
    public IReadOnlyList<RegulatorRequirement> ForRegulator(Regulator regulator) =>
        RegulatorRequirementCatalog.ForRegulator(regulator);

    /// <inheritdoc />
    public IReadOnlyList<RegulatorRequirement> ForCategory(RegulatorCategory category) =>
        RegulatorRequirementCatalog.ForCategory(category);

    /// <inheritdoc />
    public RegulatorCategory CategoryFor(Regulator regulator) =>
        RegulatorCategoryCatalog.ForRegulator(regulator);

    /// <inheritdoc />
    public IReadOnlyList<RegulatorRequirement> ForIndustry(IndustryDefinition industry) =>
        RegulatorRequirementMapping.For(industry);

    /// <inheritdoc />
    public IReadOnlyList<RegulatorRequirement> ForIndustryName(string name) =>
        RegulatorRequirementMapping.ByIndustryName(name);
}
