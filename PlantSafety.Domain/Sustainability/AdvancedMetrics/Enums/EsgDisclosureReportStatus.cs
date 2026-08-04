// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EsgDisclosureReportStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the EsgDisclosureReport aggregate root. Published reports
//                  are immutable; corrections are made only via a superseding versioned report.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;

/// <summary>Lifecycle states for the EsgDisclosureReport aggregate root.</summary>
public enum EsgDisclosureReportStatus
{
    /// <summary>Report has been generated and is still editable prior to publication.</summary>
    Draft = 0,

    /// <summary>Report has been published and is now immutable.</summary>
    Published = 1,

    /// <summary>Report has been superseded by a newer versioned report and is retained for audit trail.</summary>
    Superseded = 2,
}
