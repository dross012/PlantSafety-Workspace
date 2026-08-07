// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           CreateIncidentReportResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateIncidentReportCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Commands;

/// <summary>Result of a successful <see cref="CreateIncidentReportCommand"/> execution.</summary>
/// <param name="IncidentReportId">Identity of the newly created IncidentReport aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
/// <param name="IsReportableToPhmsa">Whether the report meets the PHMSA reporting threshold, as computed by the aggregate.</param>
public sealed record CreateIncidentReportResult(
    Guid IncidentReportId,
    string Status,
    bool IsReportableToPhmsa);
