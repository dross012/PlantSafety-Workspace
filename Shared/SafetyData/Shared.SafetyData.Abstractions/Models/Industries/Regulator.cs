// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           Regulator.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Canonical set of regulators referenced by IndustryDefinition
//                  entries in IndustryRegistry. Enum member names are
//                  identifier-safe (hyphens replaced with underscores); the
//                  exact regulator code (matching the source taxonomy verbatim,
//                  including hyphens) is recovered via RegulatorExtensions.Code().
//                  Additive-only evolution -- existing members must never be
//                  renamed or removed.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// A regulatory body or regulatory regime referenced by one or more
/// <see cref="IndustryDefinition"/> entries.
/// </summary>
public enum Regulator
{
    /// <summary>Occupational Safety and Health Administration.</summary>
    OSHA,

    /// <summary>U.S. Environmental Protection Agency (general).</summary>
    EPA,

    /// <summary>EPA -- Resource Conservation and Recovery Act.</summary>
    EPA_RCRA,

    /// <summary>EPA -- Toxic Substances Control Act.</summary>
    EPA_TSCA,

    /// <summary>Pipeline and Hazardous Materials Safety Administration.</summary>
    PHMSA,

    /// <summary>National Fire Protection Association.</summary>
    NFPA,

    /// <summary>NFPA combustible-dust standards.</summary>
    NFPA_Dust,

    /// <summary>Federal Motor Carrier Safety Administration.</summary>
    FMCSA,

    /// <summary>Department of Homeland Security -- Chemical Facility Anti-Terrorism Standards.</summary>
    DHS_CFATS,

    /// <summary>U.S. Coast Guard.</summary>
    USCG,

    /// <summary>Food and Drug Administration.</summary>
    FDA,

    /// <summary>ANSI/ISO consensus standards.</summary>
    ANSI_ISO,

    /// <summary>Bureau of Alcohol, Tobacco, Firearms and Explosives.</summary>
    ATF,

    /// <summary>Department of Defense.</summary>
    DoD,

    /// <summary>Mine Safety and Health Administration.</summary>
    MSHA,
}

/// <summary>
/// Recovers the exact, verbatim regulator code (as published in the source
/// industry taxonomy) for a <see cref="Regulator"/> value.
/// </summary>
public static class RegulatorExtensions
{
    /// <summary>Returns the exact regulator code string, including hyphens.</summary>
    public static string Code(this Regulator regulator) => regulator switch
    {
        Regulator.OSHA => "OSHA",
        Regulator.EPA => "EPA",
        Regulator.EPA_RCRA => "EPA-RCRA",
        Regulator.EPA_TSCA => "EPA-TSCA",
        Regulator.PHMSA => "PHMSA",
        Regulator.NFPA => "NFPA",
        Regulator.NFPA_Dust => "NFPA-Dust",
        Regulator.FMCSA => "FMCSA",
        Regulator.DHS_CFATS => "DHS-CFATS",
        Regulator.USCG => "USCG",
        Regulator.FDA => "FDA",
        Regulator.ANSI_ISO => "ANSI-ISO",
        Regulator.ATF => "ATF",
        Regulator.DoD => "DoD",
        Regulator.MSHA => "MSHA",
        _ => throw new ArgumentOutOfRangeException(nameof(regulator), regulator, "Unknown regulator."),
    };
}
