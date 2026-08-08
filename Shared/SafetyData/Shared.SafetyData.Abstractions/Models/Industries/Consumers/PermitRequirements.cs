// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           PermitRequirements.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Industry -> Permit/filing requirement
//                  mapping, derived entirely from IndustryDefinition.Regulators
//                  via IndustryRegistry.All. Introduces no new industries or
//                  regulators -- PermitType is a 1:1 restatement of the
//                  existing Regulator enum as the canonical permit/filing a
//                  tenant must hold with that regulator (see
//                  PermitTypeCatalog.ForRegulator). Additive-only evolution --
//                  PermitType members must never be renamed or removed, and
//                  ForRegulator must remain a total, 1:1 mapping over every
//                  Regulator value.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// A permit or regulatory filing requirement, derived 1:1 from a single
/// <see cref="Regulator"/> value.
/// </summary>
public enum PermitType
{
    /// <summary>OSHA workplace safety compliance program (from <see cref="Regulator.OSHA"/>).</summary>
    OshaComplianceProgram,

    /// <summary>EPA general environmental operating permit (from <see cref="Regulator.EPA"/>).</summary>
    EpaEnvironmentalOperatingPermit,

    /// <summary>EPA RCRA hazardous waste generator ID (from <see cref="Regulator.EPA_RCRA"/>).</summary>
    RcraHazardousWasteIdNumber,

    /// <summary>EPA TSCA chemical substance reporting (from <see cref="Regulator.EPA_TSCA"/>).</summary>
    TscaChemicalReporting,

    /// <summary>PHMSA hazmat shipper/carrier registration (from <see cref="Regulator.PHMSA"/>).</summary>
    PhmsaHazmatRegistration,

    /// <summary>NFPA fire code occupancy permit (from <see cref="Regulator.NFPA"/>).</summary>
    NfpaFireCodePermit,

    /// <summary>NFPA combustible dust hazard analysis (from <see cref="Regulator.NFPA_Dust"/>).</summary>
    NfpaDustHazardAnalysis,

    /// <summary>FMCSA motor carrier operating authority (from <see cref="Regulator.FMCSA"/>).</summary>
    FmcsaMotorCarrierOperatingAuthority,

    /// <summary>DHS CFATS facility security plan (from <see cref="Regulator.DHS_CFATS"/>).</summary>
    DhsCfatsFacilitySecurityPlan,

    /// <summary>USCG facility security plan (from <see cref="Regulator.USCG"/>).</summary>
    UscgFacilitySecurityPlan,

    /// <summary>FDA establishment registration (from <see cref="Regulator.FDA"/>).</summary>
    FdaEstablishmentRegistration,

    /// <summary>ANSI/ISO consensus-standard conformance certification (from <see cref="Regulator.ANSI_ISO"/>).</summary>
    AnsiIsoConformanceCertification,

    /// <summary>ATF explosives manufacturing/storage license (from <see cref="Regulator.ATF"/>).</summary>
    AtfExplosivesLicense,

    /// <summary>DoD facility security clearance (from <see cref="Regulator.DoD"/>).</summary>
    DodFacilityClearance,

    /// <summary>MSHA mine ID registration (from <see cref="Regulator.MSHA"/>).</summary>
    MshaMineIdRegistration,
}

/// <summary>Total, 1:1 mapping from each <see cref="Regulator"/> to its <see cref="PermitType"/>.</summary>
public static class PermitTypeCatalog
{
    /// <summary>Returns the <see cref="PermitType"/> corresponding to a <see cref="Regulator"/> value.</summary>
    public static PermitType ForRegulator(Regulator regulator) => regulator switch
    {
        Regulator.OSHA => PermitType.OshaComplianceProgram,
        Regulator.EPA => PermitType.EpaEnvironmentalOperatingPermit,
        Regulator.EPA_RCRA => PermitType.RcraHazardousWasteIdNumber,
        Regulator.EPA_TSCA => PermitType.TscaChemicalReporting,
        Regulator.PHMSA => PermitType.PhmsaHazmatRegistration,
        Regulator.NFPA => PermitType.NfpaFireCodePermit,
        Regulator.NFPA_Dust => PermitType.NfpaDustHazardAnalysis,
        Regulator.FMCSA => PermitType.FmcsaMotorCarrierOperatingAuthority,
        Regulator.DHS_CFATS => PermitType.DhsCfatsFacilitySecurityPlan,
        Regulator.USCG => PermitType.UscgFacilitySecurityPlan,
        Regulator.FDA => PermitType.FdaEstablishmentRegistration,
        Regulator.ANSI_ISO => PermitType.AnsiIsoConformanceCertification,
        Regulator.ATF => PermitType.AtfExplosivesLicense,
        Regulator.DoD => PermitType.DodFacilityClearance,
        Regulator.MSHA => PermitType.MshaMineIdRegistration,
        _ => throw new ArgumentOutOfRangeException(nameof(regulator), regulator, "Unknown regulator."),
    };
}

/// <summary>
/// Deterministic Industry -> Permit/filing requirement mapping, computed from
/// <see cref="IndustryRegistry.All"/> and each industry's <see cref="IndustryDefinition.Regulators"/>.
/// </summary>
public static class PermitRequirements
{
    /// <summary>Every distinct permit type required for the given industry, in <see cref="IndustryDefinition.Regulators"/> order.</summary>
    public static IReadOnlyList<PermitType> For(IndustryDefinition industry) =>
        industry.Regulators.Select(PermitTypeCatalog.ForRegulator).Distinct().ToArray();

    /// <summary>Required permit types for every industry in <see cref="IndustryRegistry.All"/>, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<PermitType>> ByIndustryName { get; } =
        IndustryRegistry.All.ToDictionary(
            industry => industry.Name,
            For,
            StringComparer.Ordinal);
}
