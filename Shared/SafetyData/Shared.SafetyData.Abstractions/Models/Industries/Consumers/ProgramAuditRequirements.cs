// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           ProgramAuditRequirements.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Program -> Audit requirement mapping.
//                  Introduces no new audit type -- reuses the existing
//                  AuditChecklistItem record exactly as defined (see
//                  AuditChecklistRegistry.cs). One AuditChecklistItem per
//                  program, generated directly from the program's own
//                  Code/Name, at the same granularity as
//                  ProgramSopRequirements.cs and
//                  ProgramTrainingRequirements.cs (an auditor verifying LOTO
//                  compliance needs a LOTO-specific checklist item, not the
//                  single generic "verify OSHA compliance" item every OSHA
//                  program would otherwise share). Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Program -> Audit requirement mapping, computed from <see cref="ProgramCatalog.All"/>.
/// </summary>
public static class ProgramAuditRequirements
{
    /// <summary>The <see cref="AuditChecklistItem"/> for the given program.</summary>
    public static AuditChecklistItem For(ProgramDefinition program) =>
        new($"AUDIT-{program.Code}", $"Verify {program.Name} program compliance.");

    /// <summary>Audit requirements for every program in <see cref="ProgramCatalog.All"/>, keyed by <see cref="ProgramDefinition.Code"/>.</summary>
    public static IReadOnlyDictionary<string, AuditChecklistItem> ByProgramCode { get; } =
        ProgramCatalog.All.ToDictionary(program => program.Code, For, StringComparer.Ordinal);
}
