// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EsgDisclosureReportLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the EsgDisclosureReport aggregate.
//                  Enforces the module's data-governance rule: published reports are immutable;
//                  corrections happen only via Supersede -> a new draft version.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.EsgDisclosureReport"/> aggregate.</summary>
public static class EsgDisclosureReportLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the report may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(EsgDisclosureReportStatus current) => current == EsgDisclosureReportStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the report may be superseded from <paramref name="current"/>.</summary>
    public static bool CanSupersede(EsgDisclosureReportStatus current) => current == EsgDisclosureReportStatus.Published;
}
