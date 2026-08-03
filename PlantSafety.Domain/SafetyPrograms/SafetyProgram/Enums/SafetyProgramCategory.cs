// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramCategory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Canonical categories a SafetyProgram may belong to. Deliberately a small,
//                  closed set (mirrors the UI's original DTO comment: "Training, Behavioral,
//                  Compliance, Recognition") rather than the 17/10 fine-grained sub-domain
//                  categories used by SafetyPolicies/SafetyProcedures -- a Program is a
//                  cross-cutting rollup, not a leaf document. The Application layer's detail
//                  composition handler maps each of these onto a curated set of representative
//                  SafetyPolicies/SafetyProcedures/SafetyCulture read-model queries (see
//                  GetSafetyProgramDetailQueryHandler). Additive-only evolution: new categories
//                  may be appended; existing ones must not be renumbered.
// ============================================================================
namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;

/// <summary>Canonical categories a SafetyProgram may belong to.</summary>
public enum SafetyProgramCategory
{
    /// <summary>Training and competency programs (e.g. onboarding, refresher courses).</summary>
    Training = 0,

    /// <summary>Behavioral/culture programs (e.g. behavior-based safety observation campaigns).</summary>
    Behavioral = 1,

    /// <summary>Compliance/audit programs (e.g. regulatory readiness campaigns).</summary>
    Compliance = 2,

    /// <summary>Recognition and reinforcement programs (e.g. safety suggestion/reward campaigns).</summary>
    Recognition = 3,
}
