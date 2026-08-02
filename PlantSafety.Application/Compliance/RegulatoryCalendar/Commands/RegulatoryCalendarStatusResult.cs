// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           RegulatoryCalendarStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the RegulatoryCalendar lifecycle-transition commands
//                  (Activate, RecordCompliance, Close, Archive). Mirrors PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Commands;

/// <summary>Result payload shared by RegulatoryCalendar lifecycle-transition commands.</summary>
/// <param name="RegulatoryCalendarId">Identity of the affected RegulatoryCalendar aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record RegulatoryCalendarStatusResult(Guid RegulatoryCalendarId, string Status);
