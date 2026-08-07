// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Segregation
//  Layer:          Domain
//  File:           SegregationSeverity.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Severity of a segregation violation between two hazard classes in a load.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Segregation.Enums;

/// <summary>Severity of a segregation violation between two hazard classes in a proposed load.</summary>
public enum SegregationSeverity
{
    /// <summary>Softer "away from" recommendation -- not strictly prohibited together.</summary>
    Advisory = 0,

    /// <summary>Must not be loaded/stored/transported together at all (49 CFR 177.848).</summary>
    Prohibited = 1,
}
