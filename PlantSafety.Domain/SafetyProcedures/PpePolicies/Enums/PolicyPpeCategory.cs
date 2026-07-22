// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PolicyPpeCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of PPE categories governed by a PPE policy document, per the
//                  PlantSafety Program Structure PPE Policies component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Enums;

/// <summary>Classification of PPE categories governed by a PPE policy document.</summary>
public enum PolicyPpeCategory
{
    /// <summary>Head protection.</summary>
    HeadProtection = 0,

    /// <summary>Eye/face protection.</summary>
    EyeFaceProtection = 1,

    /// <summary>Hand protection.</summary>
    HandProtection = 2,

    /// <summary>Hearing protection.</summary>
    HearingProtection = 3,

    /// <summary>Foot protection.</summary>
    FootProtection = 4,
}
