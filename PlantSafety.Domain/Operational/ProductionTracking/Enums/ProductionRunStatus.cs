// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionRunStatus.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the ProductionRun aggregate root. Vocabulary matches
//                  PlantSafety.UI.Modules.ProductionTracking.Models.ProductionRunRowDto.Status's
//                  documented set (Running, Paused, Completed, Failed, Pending) -- the UI DTO this
//                  module now backs for real. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.ProductionTracking.Enums;

/// <summary>Lifecycle states for the ProductionRun aggregate root.</summary>
public enum ProductionRunStatus
{
    /// <summary>Run has been scheduled/registered but has not yet started.</summary>
    Pending = 0,

    /// <summary>Run is actively producing.</summary>
    Running = 1,

    /// <summary>Run is temporarily paused (e.g. changeover, unplanned stop) without being ended.</summary>
    Paused = 2,

    /// <summary>Run finished successfully.</summary>
    Completed = 3,

    /// <summary>Run ended abnormally.</summary>
    Failed = 4,
}
