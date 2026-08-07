// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           IncidentReportStatusResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the IncidentReport lifecycle-transition commands
//                  (Submit, BeginReview, Close). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Commands;

/// <summary>Result payload shared by IncidentReport lifecycle-transition commands.</summary>
/// <param name="IncidentReportId">Identity of the affected IncidentReport aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record IncidentReportStatusResult(Guid IncidentReportId, string Status);
