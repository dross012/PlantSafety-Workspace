// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           ShiftType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Shift classification a CognitiveLoadLedger is scoped to. Supports Scheduling's
//                  shift-pattern correlation integration hook. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;

/// <summary>Shift classification a CognitiveLoadLedger is scoped to.</summary>
public enum ShiftType
{
    /// <summary>Standard day shift.</summary>
    Day = 0,

    /// <summary>Evening/swing shift.</summary>
    Evening = 1,

    /// <summary>Overnight/night shift.</summary>
    Night = 2,

    /// <summary>Rotating shift pattern.</summary>
    Rotating = 3,
}
