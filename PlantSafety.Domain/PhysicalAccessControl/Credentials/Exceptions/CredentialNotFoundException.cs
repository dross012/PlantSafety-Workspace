// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Credential aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Exceptions;

/// <summary>Thrown when a <see cref="Credential"/> aggregate lookup finds no match.</summary>
public sealed class CredentialNotFoundException : CredentialDomainException
{
    /// <summary>Creates a not-found exception for the given Credential identity.</summary>
    public CredentialNotFoundException(CredentialId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Credential with id '{id}' was not found.",
            "Credential.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
