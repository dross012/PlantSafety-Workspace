// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Tsca
//  Layer:          Domain
//  File:           TscaChemicalStatus.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/regulatory-posture classification for a chemical substance evaluated
//                  against TSCA (15 U.S.C. §2601, 40 CFR Part 700+). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.EPA.Tsca.Enums;

/// <summary>Regulatory posture of a chemical substance under TSCA.</summary>
public enum TscaChemicalStatus
{
    /// <summary>Chemical is listed on the TSCA Chemical Substances Inventory; no new-chemical notice required.</summary>
    OnInventory = 0,

    /// <summary>Chemical is not yet on the Inventory and a Premanufacture Notice (PMN) has been submitted but is still within its review window.</summary>
    PmnPending = 1,

    /// <summary>Chemical is not on the Inventory, is planned for manufacture/import, and a PMN is required (TSCA Section 5, 40 CFR 720) but has not been properly filed.</summary>
    PmnRequired = 2,

    /// <summary>Chemical is subject to a Significant New Use Rule (SNUR) and a Significant New Use Notice (SNUN) is required (40 CFR 721) but has not been properly filed.</summary>
    SnurTriggered = 3,

    /// <summary>Chemical is subject to an EPA-imposed restriction/prohibition under TSCA Section 5(f).</summary>
    RestrictedUnderSection5f = 4,
}
