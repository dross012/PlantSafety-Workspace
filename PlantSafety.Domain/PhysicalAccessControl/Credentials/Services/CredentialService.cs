// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for Credential. Coordinates logic that cannot live on a
//                  single aggregate (e.g. cross-credential expiration sweeps).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Services;

/// <summary>Domain service for Credential. Coordinates logic that cannot live on a single aggregate.</summary>
public class CredentialService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
