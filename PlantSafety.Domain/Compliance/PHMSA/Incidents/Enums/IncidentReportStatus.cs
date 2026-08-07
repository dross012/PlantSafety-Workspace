// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportStatus.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle statuses for the IncidentReport aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.IncidentReport"/> aggregate root.</summary>
public enum IncidentReportStatus
{
    /// <summary>Report has been created but not yet submitted for review.</summary>
    Draft = 0,

    /// <summary>Report has been submitted and awaits review.</summary>
    Submitted = 1,

    /// <summary>Report is actively being reviewed.</summary>
    UnderReview = 2,

    /// <summary>Report has been closed with no further action required.</summary>
    Closed = 3,
}
