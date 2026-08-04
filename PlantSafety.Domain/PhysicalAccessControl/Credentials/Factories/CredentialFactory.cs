// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Credential aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Factories;

/// <summary>Default factory implementation for constructing <see cref="Credential"/> aggregates.</summary>
public sealed class CredentialFactory : ICredentialFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
