// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain service for HazardCommunicationRightToKnow. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Services;

/// <summary>Domain service for HazardCommunicationRightToKnow. Coordinates logic that cannot live on a single aggregate.</summary>
public class HazardCommunicationRightToKnowService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}