// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for LimitExceedance. Coordinates logic that cannot live on a single
//                  aggregate (e.g. cross-aggregate repeat-exceedance evaluation feeding
//                  RiskManagement escalation). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Services;

/// <summary>Domain service for LimitExceedance. Coordinates logic that cannot live on a single aggregate.</summary>
public class LimitExceedanceService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}