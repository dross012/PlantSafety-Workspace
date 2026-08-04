// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EsgDisclosureFramework.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Regulatory/voluntary disclosure framework(s) an EsgDisclosureReport is
//                  structured to support. Flags enum: a single report may target more than one
//                  framework simultaneously.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;

/// <summary>Disclosure framework(s) an EsgDisclosureReport supports.</summary>
[Flags]
public enum EsgDisclosureFramework
{
    /// <summary>No framework selected.</summary>
    None = 0,

    /// <summary>GHG Protocol Corporate Accounting and Reporting Standard.</summary>
    GhgProtocol = 1,

    /// <summary>EU Corporate Sustainability Reporting Directive.</summary>
    Csrd = 2,

    /// <summary>SEC climate-related disclosure rules.</summary>
    SecClimateDisclosure = 4,
}
