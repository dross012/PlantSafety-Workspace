// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           ObstructionSeverity.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Severity classification for a recorded route obstruction.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums;

/// <summary>Severity classification for a recorded route obstruction.</summary>
public enum ObstructionSeverity
{
    /// <summary>Route remains passable with a minor delay.</summary>
    Minor = 0,

    /// <summary>Route capacity is materially reduced.</summary>
    Moderate = 1,

    /// <summary>Route is impassable and must not be used.</summary>
    Severe = 2,
}
