// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Application
//  File:           ShiftFatigueStatusResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the ShiftFatigue lifecycle-transition commands
//                  (Activate, ReassessRisk, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Scheduling.ShiftFatigue.Commands;

/// <summary>Result payload shared by ShiftFatigue lifecycle-transition commands.</summary>
/// <param name="ShiftFatigueId">Identity of the affected ShiftFatigue aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record ShiftFatigueStatusResult(Guid ShiftFatigueId, string Status);
