// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           ProgramCatalog.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic, in-memory catalog of every Safety Program,
//                  built 1:1 from RegulatorRequirementCatalog.All -- every
//                  regulatory requirement a facility must satisfy already
//                  represents something it must operationally implement as a
//                  program (a permit-required confined space program, an SPCC
//                  program, a CFATS site security plan, etc.), so this
//                  catalog is the complete 34-entry set, not a hand-picked
//                  subset. Code is built the same way RegulatorSopRequirements
//                  and RegulatorTrainingRequirements build theirs: regulator
//                  name + 1-based position within
//                  RegulatorRequirementCatalog.ForRegulator's fixed order.
//
//                  PrimaryFlag derivation (PrimaryFlagFor): a per-regulator
//                  rule, not 34 independent per-item guesses --
//                    - EPA, EPA_RCRA, EPA_TSCA -> EnvironmentalPermits
//                      (literally that flag's own definition: "subject to
//                      environmental permitting requirements").
//                    - NFPA -> Flammables (Flammable Liquids Code, Hot Work,
//                      Fire Protection Systems are all fire/flammability
//                      domain); NFPA_Dust -> Dust (Combustible Dust Hazard
//                      Analysis).
//                    - PHMSA, USCG -> HazmatShipping (both regulate hazmat/
//                      chemical transfer -- shipping, packaging, placarding
//                      for PHMSA; bulk liquid transfer, waterfront facility
//                      security for USCG -- and every Water/Marine Interface
//                      industry in IndustryRegistry.All that carries USCG
//                      also carries HazmatShipping, so this mirrors data
//                      already on file, not a new correlation).
//                    - FMCSA -> Fleet (motor carrier operating authority,
//                      driver qualification, vehicle inspection are exactly
//                      what the Fleet flag represents).
//                    - DHS_CFATS -> Chemicals (Chemical Facility
//                      Anti-Terrorism Standards is a chemicals-of-interest
//                      security regime).
//                    - ANSI_ISO -> Machinery (OSHA incorporates ANSI/ISO by
//                      reference predominantly for machine safety -- see
//                      RegulatorCategory.cs's own rationale for folding
//                      ANSI_ISO into the OSHA category).
//                    - ATF, DoD -> ExplosionRisk (explosives/ammunition/
//                      energetics/munitions domain for both -- see
//                      RegulatorCategory.cs's rationale for folding DoD into
//                      the ATF category).
//                    - MSHA -> Dust (mining safety/equipment/ventilation;
//                      every Mining & Mineral Processing-family industry in
//                      IndustryRegistry.All that carries MSHA also carries
//                      Dust).
//                    - OSHA -> per-requirement, since OSHA's own five entries
//                      span genuinely different hazard domains: LOTO -> LOTO,
//                      HazCom -> Chemicals, PIT -> Machinery,
//                      Permit-Required Confined Spaces -> ConfinedSpaces,
//                      Machine Guarding -> Machinery.
//                    - FDA -> null (GMP, Sanitation, Quality Systems are
//                      quality/food-safety programs with no hazard-flag
//                      correlate in this system -- left null rather than
//                      forced onto an unrelated flag).
//                  Additive-only evolution -- this mapping must remain total
//                  over every Regulator value, and PrimaryFlagFor's rules
//                  must not change without re-documenting the rationale here.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// Deterministic, read-only catalog of every Safety Program, built 1:1 from
/// <see cref="RegulatorRequirementCatalog.All"/>.
/// </summary>
public static class ProgramCatalog
{
    /// <summary>Every Safety Program, grouped by regulator in <see cref="Regulator"/> declaration order.</summary>
    public static IReadOnlyList<ProgramDefinition> All { get; } = Enum.GetValues<Regulator>()
        .SelectMany(regulator => RegulatorRequirementCatalog.ForRegulator(regulator)
            .Select((requirement, index) => new ProgramDefinition(
                Code: $"PROG-{regulator}-{index + 1:D2}",
                Name: requirement.Name,
                Description: requirement.Description,
                PrimaryRegulator: regulator,
                Category: requirement.Category,
                PrimaryFlag: PrimaryFlagFor(requirement))))
        .ToArray();

    /// <summary>Every program drawn from the given regulator, in catalog order.</summary>
    public static IReadOnlyList<ProgramDefinition> ForRegulator(Regulator regulator) =>
        All.Where(program => program.PrimaryRegulator == regulator).ToArray();

    /// <summary>Every program whose <see cref="ProgramDefinition.PrimaryFlag"/> equals the given flag, in catalog order.</summary>
    public static IReadOnlyList<ProgramDefinition> ForFlag(SafetyFlag flag) =>
        All.Where(program => program.PrimaryFlag == flag).ToArray();

    /// <summary>Looks up a program by its exact, canonical code.</summary>
    public static ProgramDefinition? FindByCode(string code) =>
        All.FirstOrDefault(program => string.Equals(program.Code, code, StringComparison.Ordinal));

    /// <summary>The single-regulator, per-item rule documented in this file's header.</summary>
    private static SafetyFlag? PrimaryFlagFor(RegulatorRequirement requirement) => requirement.Regulator switch
    {
        Regulator.OSHA => requirement.Name switch
        {
            "Lockout/Tagout (LOTO)" => SafetyFlag.LOTO,
            "Hazard Communication (HazCom)" => SafetyFlag.Chemicals,
            "Powered Industrial Trucks (PIT)" => SafetyFlag.Machinery,
            "Permit-Required Confined Spaces" => SafetyFlag.ConfinedSpaces,
            "Machine Guarding" => SafetyFlag.Machinery,
            _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement.Name, "Unknown OSHA requirement name."),
        },
        Regulator.EPA or Regulator.EPA_RCRA or Regulator.EPA_TSCA => SafetyFlag.EnvironmentalPermits,
        Regulator.NFPA => SafetyFlag.Flammables,
        Regulator.NFPA_Dust => SafetyFlag.Dust,
        Regulator.PHMSA => SafetyFlag.HazmatShipping,
        Regulator.FMCSA => SafetyFlag.Fleet,
        Regulator.DHS_CFATS => SafetyFlag.Chemicals,
        Regulator.USCG => SafetyFlag.HazmatShipping,
        Regulator.FDA => null,
        Regulator.ANSI_ISO => SafetyFlag.Machinery,
        Regulator.ATF => SafetyFlag.ExplosionRisk,
        Regulator.DoD => SafetyFlag.ExplosionRisk,
        Regulator.MSHA => SafetyFlag.Dust,
        _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement.Regulator, "Unknown regulator."),
    };
}
