// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service for Scorecard. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;

namespace PlantSafety.Domain.Scorecards.Services;

/// <summary>Domain service for Scorecard. Coordinates logic that cannot live on a single aggregate.</summary>
public class ScorecardService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}