// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the Credential aggregate root (a badge/credential
//                  issued to an employee, contractor, or visitor).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums;

/// <summary>Lifecycle states for the <see cref="Credential"/> aggregate root.</summary>
public enum CredentialStatus
{
    /// <summary>Credential has been issued and is active for use.</summary>
    Issued = 0,

    /// <summary>Credential is temporarily suspended (access denied, not yet revoked).</summary>
    Suspended = 1,

    /// <summary>Credential has been permanently revoked.</summary>
    Revoked = 2,

    /// <summary>Credential has expired per its ExpiresAtUtc.</summary>
    Expired = 3,
}
