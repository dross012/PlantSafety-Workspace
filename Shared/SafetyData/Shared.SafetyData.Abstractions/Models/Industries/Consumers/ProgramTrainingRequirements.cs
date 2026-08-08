// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           ProgramTrainingRequirements.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Program -> Training requirement mapping.
//                  Introduces no new training type -- reuses the existing
//                  TrainingRequirement record exactly as defined (see
//                  RegulatorTrainingRequirements.cs). One TrainingRequirement
//                  per program, generated directly from the program's own
//                  Code/Name, at the same granularity as
//                  ProgramSopRequirements.cs (and for the same reason: the
//                  regulator-level RegulatorTrainingRequirements would
//                  incorrectly attribute every training topic for a regulator
//                  to a single program). Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Program -> Training requirement mapping, computed from <see cref="ProgramCatalog.All"/>.
/// </summary>
public static class ProgramTrainingRequirements
{
    /// <summary>The <see cref="TrainingRequirement"/> for the given program.</summary>
    public static TrainingRequirement For(ProgramDefinition program) =>
        new($"TRN-{program.Code}", $"{program.Name} Training");

    /// <summary>Training requirements for every program in <see cref="ProgramCatalog.All"/>, keyed by <see cref="ProgramDefinition.Code"/>.</summary>
    public static IReadOnlyDictionary<string, TrainingRequirement> ByProgramCode { get; } =
        ProgramCatalog.All.ToDictionary(program => program.Code, For, StringComparer.Ordinal);
}
