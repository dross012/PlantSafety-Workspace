// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           IRegulatoryRequirementRegistry.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-friendly abstraction over the static Regulatory
//                  Subsystem catalogs (RegulatorRequirementCatalog,
//                  RegulatorCategoryCatalog, RegulatorRequirementMapping), so
//                  consumers can take a constructor dependency instead of a
//                  static reference -- mirrors IIndustryRegistry's role for
//                  IndustryRegistry exactly. Surface covers the core
//                  requirement/category catalog only; the Regulator-first
//                  derived mappings (RegulatorSafetyModuleMapping,
//                  RegulatorSopRequirements, RegulatorPermitRequirements,
//                  RegulatorAuditRequirements, RegulatorTrainingRequirements)
//                  remain plain static classes, consumed directly, exactly as
//                  the Industry subsystem's own Consumers/ artifacts are not
//                  individually exposed through IIndustryRegistry either.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// DI-resolvable read-only view over the Regulatory Subsystem's requirement catalog
/// (<see cref="RegulatorRequirementCatalog"/>, <see cref="RegulatorCategoryCatalog"/>, and
/// <see cref="RegulatorRequirementMapping"/>).
/// </summary>
public interface IRegulatoryRequirementRegistry
{
    /// <summary>Every regulatory requirement in the catalog, in catalog order.</summary>
    IReadOnlyList<RegulatorRequirement> All { get; }

    /// <summary>Every requirement drawn from the given regulator, in catalog order.</summary>
    IReadOnlyList<RegulatorRequirement> ForRegulator(Regulator regulator);

    /// <summary>Every requirement whose regulator maps onto the given category, in catalog order.</summary>
    IReadOnlyList<RegulatorRequirement> ForCategory(RegulatorCategory category);

    /// <summary>Returns the <see cref="RegulatorCategory"/> the given regulator maps onto.</summary>
    RegulatorCategory CategoryFor(Regulator regulator);

    /// <summary>
    /// Every regulatory requirement applicable to the given industry: the union of
    /// <see cref="ForRegulator"/> for each regulator in <see cref="IndustryDefinition.Regulators"/>.
    /// </summary>
    IReadOnlyList<RegulatorRequirement> ForIndustry(IndustryDefinition industry);

    /// <summary>
    /// Every regulatory requirement applicable to the industry with the given exact, canonical
    /// name, or an empty list if no such industry exists in <see cref="IndustryRegistry.All"/>.
    /// </summary>
    IReadOnlyList<RegulatorRequirement> ForIndustryName(string name);
}
