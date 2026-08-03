// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           ConstraintSeverity.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Severity classification for a constraint rule violation raised by the
//                  predictive scheduling rule engine. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;

/// <summary>Severity classification for a scheduling constraint violation.</summary>
public enum ConstraintSeverity
{
    /// <summary>Informational only; does not block scheduling.</summary>
    Info = 0,

    /// <summary>Undesirable but tolerable; the schedule remains feasible.</summary>
    Warning = 1,

    /// <summary>Hard violation; the candidate assignment MUST be rejected.</summary>
    Blocking = 2,
}
