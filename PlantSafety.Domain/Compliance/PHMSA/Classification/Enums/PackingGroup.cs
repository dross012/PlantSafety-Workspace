// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           PackingGroup.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DOT/PHMSA packing group classification (49 CFR 172.101), indicating the
//                  degree of danger presented by a hazardous material within its hazard class.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;

/// <summary>DOT packing group, indicating degree of danger within a hazard class.</summary>
public enum PackingGroup
{
    /// <summary>No packing group assigned (some hazard classes/divisions have none, e.g. Class 2, Class 7).</summary>
    NotApplicable = 0,

    /// <summary>Packing Group I -- great danger.</summary>
    I = 1,

    /// <summary>Packing Group II -- medium danger.</summary>
    II = 2,

    /// <summary>Packing Group III -- minor danger.</summary>
    III = 3,
}
