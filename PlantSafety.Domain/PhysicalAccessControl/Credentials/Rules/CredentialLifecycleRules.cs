// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the Credential aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Credential"/> aggregate.</summary>
public static class CredentialLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the credential may be suspended from <paramref name="current"/>.</summary>
    public static bool CanSuspend(CredentialStatus current) => current == CredentialStatus.Issued;

    /// <summary>Returns <see langword="true"/> when the credential may be revoked from <paramref name="current"/>.</summary>
    public static bool CanRevoke(CredentialStatus current) =>
        current is CredentialStatus.Issued or CredentialStatus.Suspended;

    /// <summary>Returns <see langword="true"/> when the credential may naturally expire from <paramref name="current"/>.</summary>
    public static bool CanExpire(CredentialStatus current) =>
        current is CredentialStatus.Issued or CredentialStatus.Suspended;
}
