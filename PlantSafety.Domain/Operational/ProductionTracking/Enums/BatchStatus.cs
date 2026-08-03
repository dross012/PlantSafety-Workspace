// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           BatchStatus.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for a Batch produced within a ProductionRun. Vocabulary
//                  matches PlantSafety.UI.Modules.ProductionTracking.Models.BatchRowDto.Status's
//                  documented set (Active, Completed, Failed, Pending). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.ProductionTracking.Enums;

/// <summary>Lifecycle states for a Batch produced within a ProductionRun.</summary>
public enum BatchStatus
{
    /// <summary>Batch has been registered but production has not started.</summary>
    Pending = 0,

    /// <summary>Batch is actively being produced.</summary>
    Active = 1,

    /// <summary>Batch finished successfully.</summary>
    Completed = 2,

    /// <summary>Batch ended abnormally.</summary>
    Failed = 3,
}
