// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           EffluentSampleStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the EffluentSample aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Enums;

/// <summary>Lifecycle states for the <see cref="EffluentSample"/> aggregate root.</summary>
public enum EffluentSampleStatus
{
    /// <summary>Sample recorded and part of the active record.</summary>
    Recorded = 0,

    /// <summary>Sample superseded by an append-only correction record (original retained, immutable).</summary>
    Corrected = 1,

    /// <summary>Sample voided (e.g. broken chain of custody) but retained for audit.</summary>
    Voided = 2,
}