// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           ICredentialFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Credential aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces;

/// <summary>Factory interface for constructing <see cref="Credential"/> aggregates.</summary>
public interface ICredentialFactory
{
    /// <summary>Creates a new Credential aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
