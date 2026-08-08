// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.ControlSystemSafety
//  Layer:          Domain
//  File:           PerformanceLevel.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    ISO 13849-1 Performance Level (PL) classification. PL expresses the average
//                  probability of a dangerous failure per hour (PFHd) of a safety-related part of
//                  a control system, from PLa (lowest technical safety performance) to PLe
//                  (highest). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Enums;

/// <summary>ISO 13849-1 Performance Level, PLa (lowest) through PLe (highest).</summary>
public enum PerformanceLevel
{
    PLa = 0,
    PLb = 1,
    PLc = 2,
    PLd = 3,
    PLe = 4,
}
