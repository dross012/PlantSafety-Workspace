// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service for Risk. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;

namespace PlantSafety.Domain.RiskManagement.Services;

/// <summary>Domain service for Risk. Coordinates logic that cannot live on a single aggregate.</summary>
public class RiskService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}