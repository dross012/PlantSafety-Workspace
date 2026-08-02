// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           CreateEngagementResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateEngagementCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.Engagement.Commands;

/// <summary>Result of a successful <see cref="CreateEngagementCommand"/> execution.</summary>
/// <param name="EngagementId">Identity of the newly created Engagement aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateEngagementResult(
    Guid EngagementId,
    string Status);
