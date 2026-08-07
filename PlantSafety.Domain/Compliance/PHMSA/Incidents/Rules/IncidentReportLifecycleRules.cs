// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportLifecycleRules.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Guards for the IncidentReport aggregate's Draft -> Submitted ->
//                  UnderReview -> Closed lifecycle.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.IncidentReport"/> aggregate.</summary>
public static class IncidentReportLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the report may be submitted from <paramref name="current"/>.</summary>
    public static bool CanSubmit(IncidentReportStatus current) => current == IncidentReportStatus.Draft;

    /// <summary>Returns <see langword="true"/> when review may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginReview(IncidentReportStatus current) => current == IncidentReportStatus.Submitted;

    /// <summary>Returns <see langword="true"/> when the report may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(IncidentReportStatus current) => current == IncidentReportStatus.UnderReview;
}
