// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           TIHZone.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Toxic Inhalation Hazard (TIH) zone classification per 49 CFR 173.116/173.133,
//                  denoting the severity of a gas or liquid's inhalation toxicity based on its
//                  LC50 value. Only meaningful when a HazmatMaterial's IsTIH flag is set.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;

/// <summary>Toxic Inhalation Hazard zone (A = most severe .. D = least severe).</summary>
public enum TIHZone
{
    /// <summary>Zone A -- LC50 &lt;= 200 ppm.</summary>
    A = 0,

    /// <summary>Zone B -- LC50 &gt; 200 and &lt;= 1000 ppm.</summary>
    B = 1,

    /// <summary>Zone C -- LC50 &gt; 1000 and &lt;= 3000 ppm.</summary>
    C = 2,

    /// <summary>Zone D -- LC50 &gt; 3000 and &lt;= 5000 ppm.</summary>
    D = 3,
}
