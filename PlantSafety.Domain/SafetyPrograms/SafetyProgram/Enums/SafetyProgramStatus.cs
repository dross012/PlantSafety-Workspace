// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramStatus.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the SafetyProgram aggregate root. Matches the
//                  four statuses the UI's Safety Programs list/detail pages already filter
//                  and render on (Planned/Active/OnHold/Completed) -- see
//                  PlantSafety.UI/Modules/SafetyPrograms/Pages/SafetyProgramsListPage.razor.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;

/// <summary>Lifecycle states for the SafetyProgram aggregate root.</summary>
public enum SafetyProgramStatus
{
    /// <summary>Program has been defined but has not yet started.</summary>
    Planned = 0,

    /// <summary>Program is actively running.</summary>
    Active = 1,

    /// <summary>Program is temporarily paused.</summary>
    OnHold = 2,

    /// <summary>Program has concluded.</summary>
    Completed = 3,
}
