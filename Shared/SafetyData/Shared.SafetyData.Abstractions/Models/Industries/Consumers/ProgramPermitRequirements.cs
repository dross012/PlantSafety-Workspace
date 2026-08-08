// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           ProgramPermitRequirements.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Program -> Permit requirement mapping.
//                  Introduces no new permit type -- reuses the existing
//                  PermitType enum and PermitTypeCatalog.ForRegulator exactly
//                  as defined (see PermitRequirements.cs). Unlike SOPs and
//                  training, permits are inherently regulator-level in this
//                  taxonomy (PermitType is already a total 1:1 mapping from
//                  Regulator, not from individual RegulatorRequirement
//                  entries), so every program under the same regulator
//                  correctly shares the same PermitType -- e.g. all five OSHA
//                  programs point to OshaComplianceProgram, which is accurate
//                  (a single OSHA compliance program covers LOTO, HazCom,
//                  PIT, Confined Spaces, and Machine Guarding alike).
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Program -> Permit requirement mapping, computed from
/// <see cref="PermitTypeCatalog.ForRegulator"/>.
/// </summary>
public static class ProgramPermitRequirements
{
    /// <summary>The canonical <see cref="PermitType"/> required by the given program's regulator.</summary>
    public static PermitType For(ProgramDefinition program) =>
        PermitTypeCatalog.ForRegulator(program.PrimaryRegulator);

    /// <summary>Permit requirements for every program in <see cref="ProgramCatalog.All"/>, keyed by <see cref="ProgramDefinition.Code"/>.</summary>
    public static IReadOnlyDictionary<string, PermitType> ByProgramCode { get; } =
        ProgramCatalog.All.ToDictionary(program => program.Code, For, StringComparer.Ordinal);
}
