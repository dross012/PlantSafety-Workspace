// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           CreateRegulatoryCalendarResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateRegulatoryCalendarCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Commands;

/// <summary>Result of a successful <see cref="CreateRegulatoryCalendarCommand"/> execution.</summary>
/// <param name="RegulatoryCalendarId">Identity of the newly created RegulatoryCalendar aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateRegulatoryCalendarResult(
    Guid RegulatoryCalendarId,
    string Status);
