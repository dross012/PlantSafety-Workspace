// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           HazmatMaterialStatusResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the HazmatMaterial lifecycle-transition commands
//                  (Supersede, Retire). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Classification.Commands;

/// <summary>Result payload shared by HazmatMaterial lifecycle-transition commands.</summary>
/// <param name="HazmatMaterialId">Identity of the affected HazmatMaterial aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record HazmatMaterialStatusResult(Guid HazmatMaterialId, string Status);
