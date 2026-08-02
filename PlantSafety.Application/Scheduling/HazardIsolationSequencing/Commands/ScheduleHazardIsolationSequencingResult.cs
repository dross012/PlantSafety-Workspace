// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Application
//  File:           ScheduleHazardIsolationSequencingResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Result payload for ScheduleHazardIsolationSequencingCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Commands;

/// <summary>Result of a successful <see cref="ScheduleHazardIsolationSequencingCommand"/> execution.</summary>
/// <param name="HazardIsolationSequencingId">Identity of the newly created HazardIsolationSequencing aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record ScheduleHazardIsolationSequencingResult(
    Guid HazardIsolationSequencingId,
    string Status);
