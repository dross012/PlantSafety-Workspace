// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Rcra
//  Layer:          Domain
//  File:           RcraGeneratorStatus.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    RCRA (40 CFR 262) hazardous-waste generator status classification, driven by
//                  the quantity of hazardous waste generated in a calendar month:
//                    - Very Small Quantity Generator (VSQG): <= 100 kg/month hazardous waste and
//                      <= 1 kg/month acute hazardous waste.
//                    - Small Quantity Generator (SQG): 100-999 kg/month hazardous waste (and
//                      <= 1 kg/month acute hazardous waste).
//                    - Large Quantity Generator (LQG): >= 1,000 kg/month hazardous waste, OR
//                      > 1 kg/month acute hazardous waste, regardless of total quantity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.EPA.Rcra.Enums;

/// <summary>RCRA (40 CFR 262) hazardous-waste generator status classification.</summary>
public enum RcraGeneratorStatus
{
    /// <summary>Very Small Quantity Generator: &lt;= 100 kg/month hazardous waste, &lt;= 1 kg/month acute hazardous waste.</summary>
    VerySmallQuantityGenerator = 0,

    /// <summary>Small Quantity Generator: 100-999 kg/month hazardous waste, &lt;= 1 kg/month acute hazardous waste.</summary>
    SmallQuantityGenerator = 1,

    /// <summary>Large Quantity Generator: &gt;= 1,000 kg/month hazardous waste, or &gt; 1 kg/month acute hazardous waste.</summary>
    LargeQuantityGenerator = 2,
}
