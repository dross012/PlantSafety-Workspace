// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for AccessAnomaly. Coordinates logic that cannot live on a
//                  single aggregate (e.g. correlating a batch of AccessEvents into a single
//                  anomaly, or feeding RiskManagement).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Services;

/// <summary>Domain service for AccessAnomaly. Coordinates logic that cannot live on a single aggregate.</summary>
public class AccessAnomalyService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
