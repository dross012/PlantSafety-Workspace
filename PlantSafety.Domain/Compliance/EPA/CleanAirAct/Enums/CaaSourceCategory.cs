// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.CleanAirAct
//  Layer:          Domain
//  File:           CaaSourceCategory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Clean Air Act (42 U.S.C. §7401) source classification, driving Title V
//                  (40 CFR Part 70) major-source operating-permit applicability. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.EPA.CleanAirAct.Enums;

/// <summary>Clean Air Act major-source classification for an emission source.</summary>
public enum CaaSourceCategory
{
    /// <summary>Emissions fall below all major-source thresholds.</summary>
    NonMajor = 0,

    /// <summary>
    /// Major source: emits &gt;= 100 tons/year of any air pollutant, OR &gt;= 10 tons/year of any
    /// single hazardous air pollutant (HAP), OR &gt;= 25 tons/year of combined HAPs.
    /// </summary>
    MajorSource = 1,
}
