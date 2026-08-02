// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           EngagementStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Engagement lifecycle-transition commands
//                  (Review, RecordTouchpoint, Complete, Close, Archive). Mirrors
//                  PermitsStatusResult's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.Engagement.Commands;

/// <summary>Result payload shared by Engagement lifecycle-transition commands.</summary>
/// <param name="EngagementId">Identity of the affected Engagement aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record EngagementStatusResult(Guid EngagementId, string Status);
