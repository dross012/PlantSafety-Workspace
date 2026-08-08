// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           IProgramRegistry.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-friendly abstraction over the static Safety Programs
//                  subsystem catalogs (ProgramCatalog, ProgramRequirement
//                  Mapping), so consumers can take a constructor dependency
//                  instead of a static reference -- mirrors
//                  IRegulatoryRequirementRegistry's role for the Regulatory
//                  Subsystem, and IIndustryRegistry's role for the Industry
//                  subsystem, exactly. Surface covers the core catalog and
//                  industry-mapping only; the Program-first derived
//                  consumers (ProgramSopRequirements, ProgramTraining
//                  Requirements, ProgramPermitRequirements, ProgramAudit
//                  Requirements, ProgramEventRoutingRules,
//                  ProgramComplianceProfiles) remain plain static classes,
//                  consumed directly -- same precedent as the Industry and
//                  Regulatory subsystems' own Consumers/ artifacts.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// DI-resolvable read-only view over the Safety Programs subsystem's catalog
/// (<see cref="ProgramCatalog"/>) and industry mapping (<see cref="ProgramRequirementMapping"/>).
/// </summary>
public interface IProgramRegistry
{
    /// <summary>Every Safety Program in the catalog, in catalog order.</summary>
    IReadOnlyList<ProgramDefinition> All { get; }

    /// <summary>Every program drawn from the given regulator, in catalog order.</summary>
    IReadOnlyList<ProgramDefinition> ForRegulator(Regulator regulator);

    /// <summary>Every program whose primary hazard flag equals the given flag, in catalog order.</summary>
    IReadOnlyList<ProgramDefinition> ForFlag(SafetyFlag flag);

    /// <summary>Looks up a program by its exact, canonical code.</summary>
    ProgramDefinition? FindByCode(string code);

    /// <summary>
    /// Every Safety Program applicable to the given industry (see
    /// <see cref="ProgramRequirementMapping.AppliesTo"/>).
    /// </summary>
    IReadOnlyList<ProgramDefinition> ForIndustry(IndustryDefinition industry);

    /// <summary>
    /// Every Safety Program applicable to the industry with the given exact, canonical name, or
    /// an empty list if no such industry exists in <see cref="IndustryRegistry.All"/>.
    /// </summary>
    IReadOnlyList<ProgramDefinition> ForIndustryName(string name);
}
