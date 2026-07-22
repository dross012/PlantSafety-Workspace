// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain service for IncidentTrackingPerformanceEvaluation. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Services;

/// <summary>Domain service for IncidentTrackingPerformanceEvaluation. Coordinates logic that cannot live on a single aggregate.</summary>
public class IncidentTrackingPerformanceEvaluationService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}