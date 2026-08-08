// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           RegulatorCategory.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Total, 1:1-or-many-to-one mapping from every existing
//                  Regulator value to one of nine regulatory-family
//                  categories. Introduces no new regulators and no category
//                  beyond the nine named here -- every Regulator value maps
//                  onto one of them:
//                    OSHA        <- OSHA, FMCSA, ANSI_ISO
//                    EPA         <- EPA, EPA_RCRA, EPA_TSCA
//                    NFPA        <- NFPA, NFPA_Dust
//                    PHMSA       <- PHMSA
//                    FDA         <- FDA
//                    ATF         <- ATF, DoD
//                    MSHA        <- MSHA
//                    USCG        <- USCG
//                    DHS_CFATS   <- DHS_CFATS
//                  Rationale for the three non-identity foldings (all three
//                  named categories already exist among the nine -- none
//                  invented):
//                    - FMCSA -> OSHA: FMCSA governs commercial motor vehicle
//                      safety, but within a single plant that surfaces as
//                      OSHA-domain concerns already tracked elsewhere in this
//                      subsystem (PIT/forklift, yard tractor, loading dock,
//                      fleet maintenance, driver qualification/inspection
//                      recordkeeping) -- hazmat-in-transit itself is PHMSA's
//                      own, separately-mapped domain.
//                    - ANSI_ISO -> OSHA: ANSI/ISO are voluntary consensus
//                      standards, not a regulator with independent
//                      enforcement authority; OSHA incorporates ANSI/ISO
//                      standards by reference (machine guarding, LOTO, PPE,
//                      ergonomics, robotics, electrical safety), so ANSI/ISO
//                      is always subordinate to OSHA in the U.S. regulatory
//                      structure.
//                    - DoD -> ATF: every industry in IndustryRegistry.All
//                      that carries Regulator.DoD (Aerospace & Defense) also
//                      involves energetic materials/explosives/ammunition
//                      components and propulsion systems -- DoD safety
//                      requirements in that domain (MIL-STD-882, explosives
//                      safety, munitions handling) overlap ATF's regulatory
//                      domain (explosives, ammunition, storage, security).
//                  Additive-only evolution -- this mapping must remain total
//                  over every Regulator value, and none of the nine
//                  RegulatorCategory members may be renamed, removed, or
//                  added to.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// One of the nine regulatory families every <see cref="Regulator"/> value maps onto.
/// </summary>
public enum RegulatorCategory
{
    /// <summary>Occupational Safety and Health Administration family.</summary>
    OSHA,

    /// <summary>U.S. Environmental Protection Agency family.</summary>
    EPA,

    /// <summary>National Fire Protection Association family.</summary>
    NFPA,

    /// <summary>Pipeline and Hazardous Materials Safety Administration family.</summary>
    PHMSA,

    /// <summary>Food and Drug Administration family.</summary>
    FDA,

    /// <summary>Bureau of Alcohol, Tobacco, Firearms and Explosives family.</summary>
    ATF,

    /// <summary>Mine Safety and Health Administration family.</summary>
    MSHA,

    /// <summary>U.S. Coast Guard family.</summary>
    USCG,

    /// <summary>DHS Chemical Facility Anti-Terrorism Standards family.</summary>
    DHS_CFATS,
}

/// <summary>Total mapping from every <see cref="Regulator"/> value to its <see cref="RegulatorCategory"/>.</summary>
public static class RegulatorCategoryCatalog
{
    /// <summary>Returns the <see cref="RegulatorCategory"/> corresponding to a <see cref="Regulator"/> value.</summary>
    public static RegulatorCategory ForRegulator(Regulator regulator) => regulator switch
    {
        Regulator.OSHA => RegulatorCategory.OSHA,
        Regulator.FMCSA => RegulatorCategory.OSHA,
        Regulator.ANSI_ISO => RegulatorCategory.OSHA,
        Regulator.EPA => RegulatorCategory.EPA,
        Regulator.EPA_RCRA => RegulatorCategory.EPA,
        Regulator.EPA_TSCA => RegulatorCategory.EPA,
        Regulator.NFPA => RegulatorCategory.NFPA,
        Regulator.NFPA_Dust => RegulatorCategory.NFPA,
        Regulator.PHMSA => RegulatorCategory.PHMSA,
        Regulator.FDA => RegulatorCategory.FDA,
        Regulator.ATF => RegulatorCategory.ATF,
        Regulator.DoD => RegulatorCategory.ATF,
        Regulator.MSHA => RegulatorCategory.MSHA,
        Regulator.USCG => RegulatorCategory.USCG,
        Regulator.DHS_CFATS => RegulatorCategory.DHS_CFATS,
        _ => throw new ArgumentOutOfRangeException(nameof(regulator), regulator, "Unknown regulator."),
    };
}
