// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of PPE categories per the PlantSafety Program Structure PPE
//                  component: head, eye/face, hand, hearing, foot, and high-visibility protection.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Enums;

/// <summary>Classification of PPE categories.</summary>
public enum PpeCategory
{
    /// <summary>Hard hats and bump caps.</summary>
    HeadProtection = 0,

    /// <summary>Safety glasses, goggles, and face shields.</summary>
    EyeFaceProtection = 1,

    /// <summary>Cut-resistant, chemical-resistant, or heat-resistant gloves.</summary>
    HandProtection = 2,

    /// <summary>Hearing protection: earplugs and earmuffs.</summary>
    HearingProtection = 3,

    /// <summary>Steel-toe or composite-toe boots.</summary>
    FootProtection = 4,

    /// <summary>High-visibility vests and flame-resistant clothing.</summary>
    HighVisibilityClothing = 5,
}
