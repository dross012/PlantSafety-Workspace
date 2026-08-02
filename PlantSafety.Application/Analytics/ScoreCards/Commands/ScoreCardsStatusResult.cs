// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           ScoreCardsStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the ScoreCards lifecycle-transition commands
//                  (Publish, Finalize, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.ScoreCards.Commands;

/// <summary>Result payload shared by ScoreCards lifecycle-transition commands.</summary>
/// <param name="ScoreCardsId">Identity of the affected ScoreCards aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record ScoreCardsStatusResult(Guid ScoreCardsId, string Status);
