// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Application
//  File:           RecordShiftFatigueResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Result payload for RecordShiftFatigueCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Scheduling.ShiftFatigue.Commands;

/// <summary>Result of a successful <see cref="RecordShiftFatigueCommand"/> execution.</summary>
/// <param name="ShiftFatigueId">Identity of the newly created ShiftFatigue aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record RecordShiftFatigueResult(
    Guid ShiftFatigueId,
    string Status);
