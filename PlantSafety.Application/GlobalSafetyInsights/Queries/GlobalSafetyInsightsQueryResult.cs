// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GlobalSafetyInsightsQueryResult.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Result envelope for GetGlobalSafetyInsightsQuery. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.GlobalSafetyInsights.Models;

namespace PlantSafety.Application.GlobalSafetyInsights.Queries;

/// <summary>Result envelope for <see cref="GetGlobalSafetyInsightsQuery"/>.</summary>
public sealed record GlobalSafetyInsightsQueryResult(
    Guid TenantId,
    DateTime RetrievedAtUtc,
    GlobalSafetyInsightsItemDto? Latest,
    IReadOnlyList<GlobalSafetyInsightsItemDto> History);
