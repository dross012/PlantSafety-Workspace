// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real issuance lifecycle states for the Ppe aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Enums;

/// <summary>Lifecycle states for the <see cref="Ppe"/> aggregate root.</summary>
public enum PpeStatus
{
    /// <summary>Item issued but awaiting a required fit test before use.</summary>
    PendingFitTest = 0,

    /// <summary>Item is fit-tested (or fit-test not required) and in active service.</summary>
    InService = 1,

    /// <summary>Item flagged for replacement due to wear, damage, contamination, or expiry.</summary>
    FlaggedForReplacement = 2,

    /// <summary>Item retired/withdrawn from service (replaced or permanently removed).</summary>
    Retired = 3,
}
