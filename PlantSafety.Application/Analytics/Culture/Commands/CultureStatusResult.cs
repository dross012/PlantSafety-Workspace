// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Application
//  File:           CultureStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Culture lifecycle-transition commands
//                  (Review, Publish, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.Culture.Commands;

/// <summary>Result payload shared by Culture lifecycle-transition commands.</summary>
/// <param name="CultureId">Identity of the affected Culture aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record CultureStatusResult(Guid CultureId, string Status);
