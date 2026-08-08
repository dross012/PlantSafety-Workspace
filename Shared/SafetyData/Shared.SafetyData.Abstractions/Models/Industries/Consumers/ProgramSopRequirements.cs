// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           ProgramSopRequirements.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Program -> SOP requirement mapping.
//                  Introduces no new SOP type -- reuses the existing
//                  SopTemplate record exactly as defined (see
//                  SopsRegistry.cs). One SopTemplate per program, generated
//                  directly from the program's own Code/Name (not from
//                  RegulatorSopRequirements, which is keyed at the coarser
//                  regulator level and would incorrectly attribute every SOP
//                  for a regulator -- e.g. all five OSHA SOPs -- to a single
//                  program). Since ProgramCatalog is itself already 1:1 with
//                  RegulatorRequirementCatalog, deriving one SOP per program
//                  keeps the same granularity throughout. Additive-only
//                  evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Program -> SOP requirement mapping, computed from <see cref="ProgramCatalog.All"/>.
/// </summary>
public static class ProgramSopRequirements
{
    /// <summary>The <see cref="SopTemplate"/> for the given program.</summary>
    public static SopTemplate For(ProgramDefinition program) =>
        new($"SOP-{program.Code}", $"{program.Name} SOP");

    /// <summary>SOP requirements for every program in <see cref="ProgramCatalog.All"/>, keyed by <see cref="ProgramDefinition.Code"/>.</summary>
    public static IReadOnlyDictionary<string, SopTemplate> ByProgramCode { get; } =
        ProgramCatalog.All.ToDictionary(program => program.Code, For, StringComparer.Ordinal);
}
