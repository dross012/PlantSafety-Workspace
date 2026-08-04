// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the Credential lifecycle: issuance,
//                  suspension, and revocation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Events;

/// <summary>Raised when a new badge/credential is issued to a holder.</summary>
public sealed record CredentialIssuedEvent(
    CredentialEventId EventId,
    CredentialId CredentialId,
    CredentialHolderType HolderType,
    Guid HolderUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICredentialDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CredentialIssued";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a credential is temporarily suspended.</summary>
public sealed record CredentialSuspendedEvent(
    CredentialEventId EventId,
    CredentialId CredentialId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICredentialDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CredentialSuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a credential is permanently revoked.</summary>
public sealed record CredentialRevokedEvent(
    CredentialEventId EventId,
    CredentialId CredentialId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICredentialDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CredentialRevoked";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a credential naturally expires per its ExpiresAtUtc.</summary>
public sealed record CredentialExpiredEvent(
    CredentialEventId EventId,
    CredentialId CredentialId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICredentialDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CredentialExpired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
