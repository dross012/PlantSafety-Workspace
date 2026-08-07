// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           HazmatContainerStatusResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the HazmatContainer lifecycle-transition commands
//                  (ApplyMarkingsAndLabels, ApplyPlacards, Retire). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Result payload shared by HazmatContainer lifecycle-transition commands.</summary>
/// <param name="HazmatContainerId">Identity of the affected HazmatContainer aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record HazmatContainerStatusResult(Guid HazmatContainerId, string Status);
