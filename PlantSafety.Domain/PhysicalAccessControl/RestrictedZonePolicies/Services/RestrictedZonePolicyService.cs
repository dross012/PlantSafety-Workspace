// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for RestrictedZonePolicy. Coordinates logic that cannot
//                  live on a single aggregate (e.g. resolving which policy governs a given
//                  SafetyZone/AccessEvent pair).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Services;

/// <summary>Domain service for RestrictedZonePolicy. Coordinates logic that cannot live on a single aggregate.</summary>
public class RestrictedZonePolicyService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
