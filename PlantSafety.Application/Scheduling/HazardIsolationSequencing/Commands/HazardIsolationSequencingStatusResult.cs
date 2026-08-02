// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Application
//  File:           HazardIsolationSequencingStatusResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the HazardIsolationSequencing lifecycle-transition
//                  commands (Activate, FlagConflict, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Commands;

/// <summary>Result payload shared by HazardIsolationSequencing lifecycle-transition commands.</summary>
/// <param name="HazardIsolationSequencingId">Identity of the affected HazardIsolationSequencing aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record HazardIsolationSequencingStatusResult(Guid HazardIsolationSequencingId, string Status);
