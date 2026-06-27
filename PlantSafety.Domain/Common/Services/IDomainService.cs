// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           IDomainService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Marker interface for domain services. Domain services coordinate
//      cross-aggregate logic that cannot live on a single aggregate root.
//      They must not depend on infrastructure: no HTTP, no SQL, no logging.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Services;

/// <summary>Marker interface for all domain services in PlantSafety.Domain.</summary>
public interface IDomainService
{
}
