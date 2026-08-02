// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           NearMissesStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the NearMisses lifecycle commands (Create,
//                  StartReview, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult's
//                  precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.NearMisses.Commands;

/// <summary>Result payload shared by NearMisses lifecycle commands.</summary>
/// <param name="NearMissesId">Identity of the affected NearMisses aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record NearMissesStatusResult(Guid NearMissesId, string Status);
